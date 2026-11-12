using System.Net;
using System.Net.Http.Json;
using CommonTestUtilities.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Exception;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Users.Login;

public class LoginTests(PolicyDeskApplicationFactory factory) : UsersTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_RetornarTokenDoUsuario_Quando_CredenciaisForemValidas))]
    [Trait("Integration", "Users - Login")]
    public async Task Deve_RetornarTokenDoUsuario_Quando_CredenciaisForemValidas()
    {
        var (userId, registration) = await RegisterUser();

        var response = await PostWithoutToken(LoginUri, RequestLoginJsonBuilder.Build(registration.Email, registration.Senha));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var login = await response.Content.ReadFromJsonAsync<ResponseLoginJson>(TestJson.Options);
        login.ShouldNotBeNull();
        login.Nome.ShouldBe(registration.Nome.Trim());
        login.ExpiraEm.ShouldBe(Factory.Clock.GetUtcNow().UtcDateTime.AddMinutes(Factory.Jwt.ExpirationMinutes));
        var validation = await new JsonWebTokenHandler().ValidateTokenAsync(login.Token, new TokenValidationParameters
        {
            ValidIssuer = Factory.Jwt.Issuer,
            ValidAudience = Factory.Jwt.Audience,
            IssuerSigningKey = Factory.Jwt.CreateSigningKey(),
            ValidateLifetime = false
        });
        validation.IsValid.ShouldBeTrue();
        validation.Claims[JwtRegisteredClaimNames.Sub].ShouldBe(userId.ToString());
    }

    [Fact(DisplayName = nameof(Deve_RetornarNaoAutorizado_Quando_SenhaForIncorreta))]
    [Trait("Integration", "Users - Login")]
    public async Task Deve_RetornarNaoAutorizado_Quando_SenhaForIncorreta()
    {
        var (_, registration) = await RegisterUser();

        var response = await PostWithoutToken(LoginUri, RequestLoginJsonBuilder.Build(registration.Email));

        await ShouldBeInvalidCredentials(response);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNaoAutorizado_Quando_EmailNaoEstiverCadastrado))]
    [Trait("Integration", "Users - Login")]
    public async Task Deve_RetornarNaoAutorizado_Quando_EmailNaoEstiverCadastrado()
    {
        var response = await PostWithoutToken(LoginUri, RequestLoginJsonBuilder.Build());

        await ShouldBeInvalidCredentials(response);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNaoAutorizado_Quando_UsuarioEstiverInativo))]
    [Trait("Integration", "Users - Login")]
    public async Task Deve_RetornarNaoAutorizado_Quando_UsuarioEstiverInativo()
    {
        var (userId, registration) = await RegisterUser();
        var user = await DbContext.Users.SingleAsync(item => item.Id == userId);
        user.Deactivate();
        await DbContext.SaveChangesAsync();

        var response = await PostWithoutToken(LoginUri, RequestLoginJsonBuilder.Build(registration.Email, registration.Senha));

        await ShouldBeInvalidCredentials(response);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_EmailESenhaForemVazios))]
    [Trait("Integration", "Users - Login")]
    public async Task Deve_RetornarBadRequest_Quando_EmailESenhaForemVazios()
    {
        var response = await PostWithoutToken(LoginUri, RequestLoginJsonBuilder.Build(email: "", password: ""));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain(ResourcesMessageException.INVALID_EMAIL);
        body.ShouldContain(ResourcesMessageException.USER_PASSWORD_REQUIRED);
    }

    private async Task<(Guid UserId, RequestUserJson Request)> RegisterUser()
    {
        var request = RequestUserJsonBuilder.Build();
        var response = await PostWithoutToken(RegisterUri, request);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ResponseCreatedJson>(TestJson.Options);
        created.ShouldNotBeNull();
        return (created.Id, request);
    }

    private static async Task ShouldBeInvalidCredentials(HttpResponseMessage response)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain(ResourcesMessageException.USER_INVALID_CREDENTIALS);
        body.ShouldNotContain("token", Case.Insensitive);
    }
}
