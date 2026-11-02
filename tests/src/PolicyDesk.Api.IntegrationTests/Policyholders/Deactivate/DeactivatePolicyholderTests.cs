using System.Net;
using CommonTestUtilities.Requests;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Exception;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Policyholders.Deactivate;

public class DeactivatePolicyholderTests(PolicyDeskApplicationFactory factory) : PolicyholdersTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_Inativar_Quando_NaoHouverApoliceAtiva))]
    [Trait("Integration", "Policyholders - Inativação")]
    public async Task Deve_Inativar_Quando_NaoHouverApoliceAtiva()
    {
        var policyholder = await RegisterPolicyholder();

        var response = await Delete($"{RequestUri}/{policyholder.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await FindById(policyholder.Id)).Active.ShouldBeFalse();
        (await Get($"{RequestUri}/{policyholder.Id}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_HouverApoliceAtiva))]
    [Trait("Integration", "Policyholders - Inativação")]
    public async Task Deve_RetornarBadRequest_Quando_HouverApoliceAtiva()
    {
        var policyholder = await RegisterPolicyholder();
        (await RegisterPolicy(policyholder.Id)).StatusCode.ShouldBe(HttpStatusCode.Created);

        var response = await Delete($"{RequestUri}/{policyholder.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await FindById(policyholder.Id)).Active.ShouldBeTrue();
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_HouverApoliceComVigenciaFutura))]
    [Trait("Integration", "Policyholders - Inativação")]
    public async Task Deve_RetornarBadRequest_Quando_HouverApoliceComVigenciaFutura()
    {
        var policyholder = await RegisterPolicyholder();
        var today = DateTime.UtcNow.Date;
        (await Post("/api/apolices", RequestPolicyJsonBuilder.Build(
                policyholder.Id, startDate: today.AddDays(10), endDate: today.AddDays(40))))
            .StatusCode.ShouldBe(HttpStatusCode.Created);

        var response = await Delete($"{RequestUri}/{policyholder.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain(ResourcesMessageException.POLICYHOLDER_HAS_ACTIVE_POLICY);
        (await FindById(policyholder.Id)).Active.ShouldBeTrue();
    }

    [Fact(DisplayName = nameof(Deve_ManterConsistencia_Quando_InativarECadastrarApoliceAoMesmoTempo))]
    [Trait("Integration", "Policyholders - Concorrência")]
    public async Task Deve_ManterConsistencia_Quando_InativarECadastrarApoliceAoMesmoTempo()
    {
        var policyholder = await RegisterPolicyholder();

        await Task.WhenAll(
            Delete($"{RequestUri}/{policyholder.Id}"),
            Post("/api/apolices", RequestPolicyJsonBuilder.Build(policyholder.Id)));

        // Qualquer que seja a ordem, nunca pode sobrar um segurado inativo com apólice
        var saved = await FindById(policyholder.Id);
        var hasPolicy = await DbContext.Policies.AsNoTracking()
            .AnyAsync(policy => policy.PolicyholderId == policyholder.Id);
        (saved.Active || !hasPolicy).ShouldBeTrue(
            $"Segurado ativo: {saved.Active}; possui apólice: {hasPolicy}");
    }

    [Fact(DisplayName = nameof(Deve_RetornarNotFound_Quando_SeguradoNaoExistir))]
    [Trait("Integration", "Policyholders - Inativação")]
    public async Task Deve_RetornarNotFound_Quando_SeguradoNaoExistir()
    {
        var response = await Delete($"{RequestUri}/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
