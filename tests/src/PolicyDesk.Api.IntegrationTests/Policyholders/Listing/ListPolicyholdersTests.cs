using System.Net;
using System.Net.Http.Json;
using PolicyDesk.Communication.Responses;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Policyholders.Listing;

public class ListPolicyholdersTests(PolicyDeskApplicationFactory factory) : PolicyholdersTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_ListarSomenteSeguradosAtivos_Quando_Listar))]
    [Trait("Integration", "Policyholders - Listagem")]
    public async Task Deve_ListarSomenteSeguradosAtivos_Quando_Listar()
    {
        var active = await RegisterPolicyholder();
        var inactive = await RegisterPolicyholder();
        await Delete($"{RequestUri}/{inactive.Id}");

        var response = await Get(RequestUri);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<ResponsePolicyholdersListingJson>>();
        items.ShouldNotBeNull();
        items.ShouldContain(item => item.Id == active.Id);
        items.ShouldNotContain(item => item.Id == inactive.Id);
    }

    [Fact(DisplayName = nameof(Deve_RetornarSegurado_Quando_BuscarPorId))]
    [Trait("Integration", "Policyholders - Listagem")]
    public async Task Deve_RetornarSegurado_Quando_BuscarPorId()
    {
        var policyholder = await RegisterPolicyholder();

        var response = await Get($"{RequestUri}/{policyholder.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var item = await response.Content.ReadFromJsonAsync<ResponsePolicyholdersListingJson>();
        item.ShouldNotBeNull();
        item.Cpf.ShouldBe(policyholder.Cpf);
        item.Email.ShouldBe(policyholder.Email);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNotFound_Quando_BuscarPorIdInexistente))]
    [Trait("Integration", "Policyholders - Listagem")]
    public async Task Deve_RetornarNotFound_Quando_BuscarPorIdInexistente()
    {
        var response = await Get($"{RequestUri}/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
