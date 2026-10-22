using System.Net;
using CommonTestUtilities.Requests;
using PolicyDesk.Exception;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Policyholders.Register;

public class RegisterPolicyholderTests(PolicyDeskApplicationFactory factory) : PolicyholdersTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_Cadastrar_Quando_RequisicaoForValida))]
    [Trait("Integration", "Policyholders - Cadastro")]
    public async Task Deve_Cadastrar_Quando_RequisicaoForValida()
    {
        var request = RequestPolicyholdersJsonBuilder.Build();

        var response = await Post(RequestUri, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var saved = await FindByCpf(request.Cpf);
        saved.Active.ShouldBeTrue();
        saved.Name.ShouldBe(request.Name.Trim());
        saved.Email.ShouldBe(request.Email.ToLowerInvariant());
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_CpfForInvalido))]
    [Trait("Integration", "Policyholders - Cadastro")]
    public async Task Deve_RetornarBadRequest_Quando_CpfForInvalido()
    {
        var request = RequestPolicyholdersJsonBuilder.Build(cpf: "111.111.111-11");

        var response = await Post(RequestUri, request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain(ResourcesMessageException.INVALID_CPF);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_CpfJaEstiverAtivo))]
    [Trait("Integration", "Policyholders - Cadastro")]
    public async Task Deve_RetornarBadRequest_Quando_CpfJaEstiverAtivo()
    {
        var existing = await RegisterPolicyholder();

        var response = await Post(RequestUri, RequestPolicyholdersJsonBuilder.Build(cpf: existing.Cpf));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_EmailJaEstiverAtivo))]
    [Trait("Integration", "Policyholders - Cadastro")]
    public async Task Deve_RetornarBadRequest_Quando_EmailJaEstiverAtivo()
    {
        var existing = await RegisterPolicyholder();

        var response = await Post(RequestUri, RequestPolicyholdersJsonBuilder.Build(email: existing.Email));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = nameof(Deve_Reativar_Quando_CpfEstiverInativo))]
    [Trait("Integration", "Policyholders - Reativação")]
    public async Task Deve_Reativar_Quando_CpfEstiverInativo()
    {
        var original = await RegisterPolicyholder();
        (await Delete($"{RequestUri}/{original.Id}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var request = RequestPolicyholdersJsonBuilder.Build(cpf: original.Cpf);

        var response = await Post(RequestUri, request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
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

        var response = await Post(RequestUri, RequestPolicyholdersJsonBuilder.Build(cpf: inactive.Cpf, email: other.Email));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await FindById(inactive.Id)).Active.ShouldBeFalse();
    }
}
