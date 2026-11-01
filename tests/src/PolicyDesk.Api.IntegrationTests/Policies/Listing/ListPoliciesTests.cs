using System.Net;
using System.Net.Http.Json;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Enums;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Policies.Listing;

public class ListPoliciesTests(PolicyDeskApplicationFactory factory) : PoliciesTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_ListarApolicesDoSegurado_Quando_FiltrarPorSegurado))]
    [Trait("Integration", "Policies - Listagem")]
    public async Task Deve_ListarApolicesDoSegurado_Quando_FiltrarPorSegurado()
    {
        var holder = await RegisterPolicyholder();
        var otherHolder = await RegisterPolicyholder();
        var expected = await RegisterPolicyForHolder(holder);
        await RegisterPolicyForHolder(otherHolder);

        var response = await Get($"/api/apolices?seguradoId={holder.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<ResponsePolicyJson>>();
        items.ShouldNotBeNull();
        items.Select(item => item.Id).ShouldBe([expected.Id]);
    }

    [Fact(DisplayName = nameof(Deve_FiltrarApolicesPorStatus_Quando_StatusForInformado))]
    [Trait("Integration", "Policies - Listagem")]
    public async Task Deve_FiltrarApolicesPorStatus_Quando_StatusForInformado()
    {
        var holder = await RegisterPolicyholder();
        var active = await RegisterPolicyForHolder(holder);
        var cancelled = await RegisterPolicyForHolder(holder);
        (await Post($"/api/apolices/{cancelled.Id}/cancelamento", new { }))
            .StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var response = await Get($"/api/apolices?status={PolicyStatus.Cancelled}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<ResponsePolicyJson>>();
        items.ShouldNotBeNull();
        items.Select(item => item.Id).ShouldContain(cancelled.Id);
        items.Select(item => item.Id).ShouldNotContain(active.Id);
    }

    [Fact(DisplayName = nameof(Deve_RetornarApolice_Quando_BuscarPorId))]
    [Trait("Integration", "Policies - Listagem")]
    public async Task Deve_RetornarApolice_Quando_BuscarPorId()
    {
        var policy = await RegisterPolicyForNewHolder();

        var response = await Get($"/api/apolices/{policy.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var item = await response.Content.ReadFromJsonAsync<ResponsePolicyJson>();
        item.ShouldNotBeNull();
        item.Id.ShouldBe(policy.Id);
        item.SeguradoId.ShouldBe(policy.PolicyholderId);
        item.ValorCobertura.ShouldBe(policy.CoverageAmount);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNotFound_Quando_BuscarApoliceInexistente))]
    [Trait("Integration", "Policies - Listagem")]
    public async Task Deve_RetornarNotFound_Quando_BuscarApoliceInexistente()
    {
        var response = await Get($"/api/apolices/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
