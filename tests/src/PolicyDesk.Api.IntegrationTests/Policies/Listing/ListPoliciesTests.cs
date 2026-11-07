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
        var items = await response.Content.ReadFromJsonAsync<ResponsePagedJson<ResponsePolicyJson>>();
        items.ShouldNotBeNull();
        items.Itens.Select(item => item.Id).ShouldBe([expected.Id]);
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

        var response = await Get($"/api/apolices?seguradoId={holder.Id}&status={PolicyStatus.Cancelled}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<ResponsePagedJson<ResponsePolicyJson>>();
        items.ShouldNotBeNull();
        items.Itens.Select(item => item.Id).ShouldBe([cancelled.Id]);
        items.Itens.Select(item => item.Id).ShouldNotContain(active.Id);
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

    [Fact(DisplayName = nameof(Deve_PaginarNaOrdemDeCadastro_Quando_InformarPaginaETamanho))]
    [Trait("Integration", "Policies - Listagem")]
    public async Task Deve_PaginarNaOrdemDeCadastro_Quando_InformarPaginaETamanho()
    {
        var holder = await RegisterPolicyholder();
        var first = await RegisterPolicyForHolder(holder);
        var second = await RegisterPolicyForHolder(holder);
        var third = await RegisterPolicyForHolder(holder);

        var firstPage = await ReadPage($"/api/apolices?seguradoId={holder.Id}&pagina=1&tamanhoPagina=2");
        var secondPage = await ReadPage($"/api/apolices?seguradoId={holder.Id}&pagina=2&tamanhoPagina=2");

        firstPage.Itens.Select(item => item.Id).ShouldBe([first.Id, second.Id]);
        secondPage.Itens.Select(item => item.Id).ShouldBe([third.Id]);
        secondPage.Pagina.ShouldBe(2);
        secondPage.TamanhoPagina.ShouldBe(2);
        secondPage.TotalItens.ShouldBe(3);
        secondPage.TotalPaginas.ShouldBe(2);
    }

    [Theory(DisplayName = nameof(Deve_RetornarBadRequest_Quando_PaginacaoForInvalida))]
    [Trait("Integration", "Policies - Listagem")]
    [InlineData("pagina=0")]
    [InlineData("tamanhoPagina=0")]
    [InlineData("tamanhoPagina=101")]
    public async Task Deve_RetornarBadRequest_Quando_PaginacaoForInvalida(string query)
    {
        var response = await Get($"/api/apolices?{query}");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNotFound_Quando_BuscarApoliceInexistente))]
    [Trait("Integration", "Policies - Listagem")]
    public async Task Deve_RetornarNotFound_Quando_BuscarApoliceInexistente()
    {
        var response = await Get($"/api/apolices/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<ResponsePagedJson<ResponsePolicyJson>> ReadPage(string requestUri)
    {
        var response = await Get(requestUri);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<ResponsePagedJson<ResponsePolicyJson>>();
        page.ShouldNotBeNull();
        return page;
    }
}
