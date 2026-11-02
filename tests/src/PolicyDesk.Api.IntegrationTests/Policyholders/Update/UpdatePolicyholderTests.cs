using System.Net;
using CommonTestUtilities.Requests;
using PolicyDesk.Exception;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Policyholders.Update;

public class UpdatePolicyholderTests(PolicyDeskApplicationFactory factory) : PolicyholdersTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_Atualizar_Quando_RequisicaoForValida))]
    [Trait("Integration", "Policyholders - Atualização")]
    public async Task Deve_Atualizar_Quando_RequisicaoForValida()
    {
        var policyholder = await RegisterPolicyholder();
        var request = RequestPolicyholdersUpdateJsonBuilder.Build();

        var response = await Put($"{RequestUri}/{policyholder.Id}", request);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var updated = await FindById(policyholder.Id);
        updated.Name.ShouldBe(request.Name.Trim());
        updated.Email.ShouldBe(request.Email.ToLowerInvariant());
        updated.Cpf.ShouldBe(policyholder.Cpf);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_EmailForDeOutroSeguradoAtivo))]
    [Trait("Integration", "Policyholders - Atualização")]
    public async Task Deve_RetornarBadRequest_Quando_EmailForDeOutroSeguradoAtivo()
    {
        var existing = await RegisterPolicyholder();
        var other = await RegisterPolicyholder();

        var response = await Put(
            $"{RequestUri}/{other.Id}",
            RequestPolicyholdersUpdateJsonBuilder.Build(email: existing.Email.ToUpperInvariant()));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await FindById(other.Id)).Email.ShouldBe(other.Email);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequestSemAlterar_Quando_NomeForEmBranco))]
    [Trait("Integration", "Policyholders - Atualização")]
    public async Task Deve_RetornarBadRequestSemAlterar_Quando_NomeForEmBranco()
    {
        var policyholder = await RegisterPolicyholder();
        var request = RequestPolicyholdersUpdateJsonBuilder.Build();
        request.Name = "   ";

        var response = await Put($"{RequestUri}/{policyholder.Id}", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain(ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED);
        var saved = await FindById(policyholder.Id);
        saved.Name.ShouldBe(policyholder.Name);
        saved.UpdatedAt.ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequestSemAlterar_Quando_TelefoneForInvalido))]
    [Trait("Integration", "Policyholders - Atualização")]
    public async Task Deve_RetornarBadRequestSemAlterar_Quando_TelefoneForInvalido()
    {
        var policyholder = await RegisterPolicyholder();
        var request = RequestPolicyholdersUpdateJsonBuilder.Build();
        request.PhoneNumber = "123";

        var response = await Put($"{RequestUri}/{policyholder.Id}", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain(ResourcesMessageException.INVALID_PHONE);
        (await FindById(policyholder.Id)).Phone.ShouldBe(policyholder.Phone);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNotFound_Quando_SeguradoNaoExistir))]
    [Trait("Integration", "Policyholders - Atualização")]
    public async Task Deve_RetornarNotFound_Quando_SeguradoNaoExistir()
    {
        var response = await Put($"{RequestUri}/{Guid.NewGuid()}", RequestPolicyholdersUpdateJsonBuilder.Build());

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
