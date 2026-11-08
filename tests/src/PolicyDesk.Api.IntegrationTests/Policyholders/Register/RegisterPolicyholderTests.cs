using System.Net;
using System.Net.Http.Json;
using CommonTestUtilities.Requests;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Exception;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Policyholders.Register;

public class RegisterPolicyholderTests(PolicyDeskApplicationFactory factory) : PolicyholdersTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_Cadastrar_Quando_RequisicaoForValida))]
    [Trait("Integration", "Policyholders - Cadastro")]
    public async Task Deve_Cadastrar_Quando_RequisicaoForValida()
    {
        var request = RequestPolicyholderJsonBuilder.Build();

        var response = await Post(RequestUri, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var saved = await FindByCpf(request.Cpf);
        var created = await response.Content.ReadFromJsonAsync<ResponseCreatedJson>(TestJson.Options);
        created.ShouldNotBeNull();
        created.Id.ShouldBe(saved.Id);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.AbsolutePath.ShouldBe($"{RequestUri}/{saved.Id}");
        saved.Active.ShouldBeTrue();
        saved.Name.ShouldBe(request.Nome.Trim());
        saved.Email.ShouldBe(request.Email.ToLowerInvariant());
    }

    [Theory(DisplayName = nameof(Deve_Cadastrar_Quando_TelefoneTiverCodigoDePais))]
    [Trait("Integration", "Policyholders - Cadastro")]
    [InlineData("+55 (83) 98647-1687", "+5583986471687")]
    [InlineData("+44 20 7946 0958", "+442079460958")]
    public async Task Deve_Cadastrar_Quando_TelefoneTiverCodigoDePais(string phone, string expectedPhone)
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.Telefone = phone;

        var response = await Post(RequestUri, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await FindByCpf(request.Cpf)).Phone.ShouldBe(expectedPhone);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_CpfForInvalido))]
    [Trait("Integration", "Policyholders - Cadastro")]
    public async Task Deve_RetornarBadRequest_Quando_CpfForInvalido()
    {
        var request = RequestPolicyholderJsonBuilder.Build(cpf: "111.111.111-11");

        var response = await Post(RequestUri, request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain(ResourcesMessageException.INVALID_CPF);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequestSemGravar_Quando_NomeForEmBranco))]
    [Trait("Integration", "Policyholders - Cadastro")]
    public async Task Deve_RetornarBadRequestSemGravar_Quando_NomeForEmBranco()
    {
        var request = RequestPolicyholderJsonBuilder.Build();
        request.Nome = "   ";

        var response = await Post(RequestUri, request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain(ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED);
        (await DbContext.Policyholders.IgnoreQueryFilters().AnyAsync(p => p.Cpf == request.Cpf)).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_CamposObrigatoriosForemOmitidos))]
    [Trait("Integration", "Policyholders - Cadastro")]
    public async Task Deve_RetornarBadRequest_Quando_CamposObrigatoriosForemOmitidos()
    {
        var cpf = CpfGenerator.Generate();

        var response = await Post(RequestUri, new { cpf, email = "omitted.fields@example.com" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain(ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED);
        body.ShouldContain(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_REQUIRED);
        body.ShouldContain(ResourcesMessageException.POLICYHOLDER_PHONE_REQUIRED);
        (await DbContext.Policyholders.IgnoreQueryFilters().AnyAsync(p => p.Cpf == cpf)).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_CpfJaEstiverAtivo))]
    [Trait("Integration", "Policyholders - Cadastro")]
    public async Task Deve_RetornarBadRequest_Quando_CpfJaEstiverAtivo()
    {
        var existing = await RegisterPolicyholder();

        var response = await Post(RequestUri, RequestPolicyholderJsonBuilder.Build(cpf: existing.Cpf));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_EmailJaEstiverAtivo))]
    [Trait("Integration", "Policyholders - Cadastro")]
    public async Task Deve_RetornarBadRequest_Quando_EmailJaEstiverAtivo()
    {
        var existing = await RegisterPolicyholder();

        var response = await Post(RequestUri, RequestPolicyholderJsonBuilder.Build(email: existing.Email));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = nameof(Deve_Reativar_Quando_CpfEstiverInativo))]
    [Trait("Integration", "Policyholders - Reativação")]
    public async Task Deve_Reativar_Quando_CpfEstiverInativo()
    {
        var original = await RegisterPolicyholder();
        (await Delete($"{RequestUri}/{original.Id}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var request = RequestPolicyholderJsonBuilder.Build(cpf: original.Cpf);

        var response = await Post(RequestUri, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        (await response.Content.ReadFromJsonAsync<ResponseCreatedJson>(TestJson.Options))!.Id.ShouldBe(original.Id);
        var reactivated = await FindById(original.Id);
        reactivated.Active.ShouldBeTrue();
        reactivated.Email.ShouldBe(request.Email.ToLowerInvariant());
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_ReativarComEmailDeOutroSeguradoAtivo))]
    [Trait("Integration", "Policyholders - Reativação")]
    public async Task Deve_RetornarBadRequest_Quando_ReativarComEmailDeOutroSeguradoAtivo()
    {
        var inactive = await RegisterPolicyholder();
        (await Delete($"{RequestUri}/{inactive.Id}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var other = await RegisterPolicyholder();

        var response = await Post(RequestUri, RequestPolicyholderJsonBuilder.Build(cpf: inactive.Cpf, email: other.Email));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await FindById(inactive.Id)).Active.ShouldBeFalse();
    }
}
