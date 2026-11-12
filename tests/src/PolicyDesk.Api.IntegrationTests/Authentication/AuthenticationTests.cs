using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Security;
using CommonTestUtilities.Time;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using PolicyDesk.Api.Errors;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Exception;
using PolicyDesk.Infrastructure.Security.Tokens;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Authentication;

public class AuthenticationTests(PolicyDeskApplicationFactory factory) : BaseIntegrationTest(factory)
{
    private const string ProtectedUri = "/api/segurados";

    [Fact(DisplayName = nameof(Deve_RetornarNaoAutorizado_Quando_AcessarRecursoSemToken))]
    [Trait("Integration", "Autenticação - Acesso")]
    public async Task Deve_RetornarNaoAutorizado_Quando_AcessarRecursoSemToken()
    {
        var response = await GetWithoutToken(ProtectedUri);

        await ShouldBeAuthenticationRequired(response);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNaoAutorizado_Quando_TokenForAssinadoComOutraChave))]
    [Trait("Integration", "Autenticação - Acesso")]
    public async Task Deve_RetornarNaoAutorizado_Quando_TokenForAssinadoComOutraChave()
    {
        var foreignOptions = JwtOptionsBuilder.Build();
        var foreignToken = new JwtAccessTokenGenerator(Options.Create(foreignOptions), BusinessClockBuilder.Build(Factory.Clock.GetUtcNow()))
            .Generate(Guid.NewGuid()).Token;

        var response = await GetWithToken(ProtectedUri, foreignToken);

        await ShouldBeAuthenticationRequired(response);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNaoAutorizado_Quando_TokenEstiverExpirado))]
    [Trait("Integration", "Autenticação - Acesso")]
    public async Task Deve_RetornarNaoAutorizado_Quando_TokenEstiverExpirado()
    {
        var issuedAt = Factory.Clock.GetUtcNow().AddMinutes(-(JwtOptionsBuilder.ExpirationMinutes + 10));
        var expiredToken = Factory.CreateAccessToken(issuedAt: issuedAt);

        var response = await GetWithToken(ProtectedUri, expiredToken);

        await ShouldBeAuthenticationRequired(response);
    }

    [Fact(DisplayName = nameof(Deve_PermitirAcesso_Quando_UsarTokenRecebidoAoEntrar))]
    [Trait("Integration", "Autenticação - Acesso")]
    public async Task Deve_PermitirAcesso_Quando_UsarTokenRecebidoAoEntrar()
    {
        var registration = RequestUserJsonBuilder.Build();
        (await PostWithoutToken("/api/autenticacao/registrar", registration)).StatusCode.ShouldBe(HttpStatusCode.Created);
        var loginResponse = await PostWithoutToken(
            "/api/autenticacao/entrar",
            RequestLoginJsonBuilder.Build(registration.Email, registration.Senha));
        var login = await loginResponse.Content.ReadFromJsonAsync<ResponseLoginJson>(TestJson.Options);
        login.ShouldNotBeNull();

        var response = await GetWithToken(ProtectedUri, login.Token);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(DisplayName = nameof(Deve_PermitirAcessoSemToken_Quando_ConsultarSaude))]
    [Trait("Integration", "Autenticação - Acesso")]
    public async Task Deve_PermitirAcessoSemToken_Quando_ConsultarSaude()
    {
        var response = await GetWithoutToken("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact(DisplayName = nameof(Deve_DocumentarOEsquemaBearerSomenteNasRotasProtegidas_Quando_GerarOpenApi))]
    [Trait("Integration", "Autenticação - Swagger")]
    public async Task Deve_DocumentarOEsquemaBearerSomenteNasRotasProtegidas_Quando_GerarOpenApi()
    {
        using var client = Factory
            .WithWebHostBuilder(builder => builder.UseSetting("Swagger:Enabled", "true"))
            .CreateClient();
        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.GetAsync("/openapi/v1.json");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var document = await response.Content.ReadFromJsonAsync<JsonElement>();
        var scheme = document.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer");
        scheme.GetProperty("scheme").GetString().ShouldBe("bearer");
        var paths = document.GetProperty("paths");
        paths.GetProperty("/api/segurados").GetProperty("get").TryGetProperty("security", out _).ShouldBeTrue();
        paths.GetProperty("/api/autenticacao/entrar").GetProperty("post").TryGetProperty("security", out _).ShouldBeFalse();
        paths.GetProperty("/api/autenticacao/registrar").GetProperty("post").TryGetProperty("security", out _).ShouldBeFalse();
    }

    private async Task<HttpResponseMessage> GetWithToken(string requestUri, string token)
    {
        using var client = Factory.CreateAnonymousClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.GetAsync(requestUri);
    }

    private static async Task ShouldBeAuthenticationRequired(HttpResponseMessage response)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Headers.WwwAuthenticate.ToString().ShouldContain("Bearer");
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty(ProblemDetailsSetup.ErrorsKey).EnumerateArray()
            .Select(error => error.GetString())
            .ShouldBe([ResourcesMessageException.AUTHENTICATION_REQUIRED]);
        problem.GetProperty(ProblemDetailsSetup.TraceIdKey).GetString().ShouldNotBeNullOrWhiteSpace();
    }
}
