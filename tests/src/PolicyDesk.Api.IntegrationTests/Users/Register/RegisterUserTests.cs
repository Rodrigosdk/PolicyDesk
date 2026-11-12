using System.Net;
using System.Net.Http.Json;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Security;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Users.Register;

public class RegisterUserTests(PolicyDeskApplicationFactory factory) : UsersTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_Cadastrar_Quando_RequisicaoForValida))]
    [Trait("Integration", "Users - Cadastro")]
    public async Task Deve_Cadastrar_Quando_RequisicaoForValida()
    {
        var request = RequestUserJsonBuilder.Build();

        var response = await PostWithoutToken(RegisterUri, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ResponseCreatedJson>(TestJson.Options);
        created.ShouldNotBeNull();
        var saved = await FindById(created.Id);
        saved.Active.ShouldBeTrue();
        saved.Name.ShouldBe(request.Nome.Trim());
        saved.Email.ShouldBe(request.Email.ToLowerInvariant());
        saved.Password.ShouldNotBe(request.Senha);
        PasswordHashingBuilder.Build().VerifyPassword(request.Senha, saved.Password).ShouldBeTrue();
    }

    [Fact(DisplayName = nameof(Deve_NaoExporASenha_Quando_Cadastrar))]
    [Trait("Integration", "Users - Cadastro")]
    public async Task Deve_NaoExporASenha_Quando_Cadastrar()
    {
        var request = RequestUserJsonBuilder.Build();

        var response = await PostWithoutToken(RegisterUri, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldNotContain(request.Senha);
        body.ShouldNotContain("senha", Case.Insensitive);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequestSemGravar_Quando_EmailJaEstiverEmUso))]
    [Trait("Integration", "Users - Cadastro")]
    public async Task Deve_RetornarBadRequestSemGravar_Quando_EmailJaEstiverEmUso()
    {
        var existing = RequestUserJsonBuilder.Build();
        (await PostWithoutToken(RegisterUri, existing)).StatusCode.ShouldBe(HttpStatusCode.Created);

        var response = await PostWithoutToken(RegisterUri, RequestUserJsonBuilder.Build(email: existing.Email));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain(ResourcesMessageException.USER_ACTIVE_EMAIL_EXISTS);
        (await CountByEmail(existing.Email.ToLowerInvariant())).ShouldBe(1);
    }

    [Fact(DisplayName = nameof(Deve_ReativarUsuario_Quando_EmailForDeUsuarioInativadoRecentementeESenhaConferir))]
    [Trait("Integration", "Users - Cadastro")]
    public async Task Deve_ReativarUsuario_Quando_EmailForDeUsuarioInativadoRecentementeESenhaConferir()
    {
        var original = RequestUserJsonBuilder.Build();
        var userId = await RegisterAndDeactivate(original);

        var response = await PostWithoutToken(RegisterUri, RequestUserJsonBuilder.Build(email: original.Email, password: original.Senha));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ResponseCreatedJson>(TestJson.Options);
        created.ShouldNotBeNull();
        created.Id.ShouldBe(userId);
        var saved = await FindById(userId);
        saved.Active.ShouldBeTrue();
        saved.DeactivatedAt.ShouldBeNull();
        (await CountByEmail(saved.Email)).ShouldBe(1);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequestSemReativar_Quando_SenhaNaoConferir))]
    [Trait("Integration", "Users - Cadastro")]
    public async Task Deve_RetornarBadRequestSemReativar_Quando_SenhaNaoConferir()
    {
        var original = RequestUserJsonBuilder.Build();
        var userId = await RegisterAndDeactivate(original);

        var response = await PostWithoutToken(RegisterUri, RequestUserJsonBuilder.Build(email: original.Email));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain(ResourcesMessageException.USER_INVALID_CREDENTIALS);
        (await FindById(userId)).Active.ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequestComTodosOsErros_Quando_CamposForemInvalidos))]
    [Trait("Integration", "Users - Cadastro")]
    public async Task Deve_RetornarBadRequestComTodosOsErros_Quando_CamposForemInvalidos()
    {
        var request = RequestUserJsonBuilder.Build(name: "   ", email: "invalido", password: "curta");

        var response = await PostWithoutToken(RegisterUri, request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain(ResourcesMessageException.USER_NAME_REQUIRED);
        body.ShouldContain(ResourcesMessageException.INVALID_EMAIL);
        body.ShouldContain(FieldLimits.UserPasswordTooShort);
        (await CountByEmail("invalido")).ShouldBe(0);
    }

    private async Task<Guid> RegisterAndDeactivate(RequestUserJson request)
    {
        var response = await PostWithoutToken(RegisterUri, request);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ResponseCreatedJson>(TestJson.Options);
        created.ShouldNotBeNull();

        var user = await DbContext.Users.SingleAsync(item => item.Id == created.Id);
        user.Deactivate();
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        return created.Id;
    }
}
