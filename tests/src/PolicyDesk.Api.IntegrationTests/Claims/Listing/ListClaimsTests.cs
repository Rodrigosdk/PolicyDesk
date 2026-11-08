using System.Net;
using System.Net.Http.Json;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Communication.Enums;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Claims.Listing;

public class ListClaimsTests(PolicyDeskApplicationFactory factory) : ClaimsTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_ListarApenasSinistrosDaApolice_Quando_ApoliceExistir))]
    [Trait("Integration", "Claims - Listagem")]
    public async Task Deve_ListarApenasSinistrosDaApolice_Quando_ApoliceExistir()
    {
        var policy = await RegisterPolicyForNewHolder();
        var first = await RegisterClaimForPolicy(policy);
        var second = await RegisterClaimForPolicy(policy);
        await RegisterClaimForNewPolicy();

        var response = await Get($"/api/apolices/{policy.Id}/sinistros");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var claims = await response.Content.ReadFromJsonAsync<List<ResponseClaimJson>>(TestJson.Options);
        claims.ShouldNotBeNull();
        claims.Select(claim => claim.Id).ShouldBe([first.Id, second.Id], ignoreOrder: true);
        claims.ShouldAllBe(claim => claim.ApoliceId == policy.Id);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNotFound_Quando_ListarDeApoliceInexistente))]
    [Trait("Integration", "Claims - Listagem")]
    public async Task Deve_RetornarNotFound_Quando_ListarDeApoliceInexistente()
    {
        var response = await Get($"/api/apolices/{Guid.NewGuid()}/sinistros");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = nameof(Deve_RetornarSinistro_Quando_BuscarPorId))]
    [Trait("Integration", "Claims - Consulta")]
    public async Task Deve_RetornarSinistro_Quando_BuscarPorId()
    {
        var claim = await RegisterClaimForNewPolicy();

        var response = await Get($"/api/sinistros/{claim.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ResponseClaimJson>(TestJson.Options);
        body.ShouldNotBeNull();
        body.Id.ShouldBe(claim.Id);
        body.ApoliceId.ShouldBe(claim.PolicyId);
        body.Descricao.ShouldBe(claim.Description);
        body.Valor.ShouldBe(claim.Amount);
        body.Status.ShouldBe(StatusSinistro.Pendente);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNotFound_Quando_BuscarSinistroInexistente))]
    [Trait("Integration", "Claims - Consulta")]
    public async Task Deve_RetornarNotFound_Quando_BuscarSinistroInexistente()
    {
        var response = await Get($"/api/sinistros/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
