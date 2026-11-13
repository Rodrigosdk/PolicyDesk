using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories.Policies;
using PolicyDesk.Application.Policies.Listing;
using PolicyDesk.Communication.Enums;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Policies.Listing;

public class ListPoliciesUseCaseTests
{
    private static readonly RequestPaginationJson FirstPage = new();

    [Fact(DisplayName = nameof(Deve_MapearApolicesParaResposta_Quando_Listar))]
    [Trait("Application", "Policies - Listing")]
    public async Task Deve_MapearApolicesParaResposta_Quando_Listar()
    {
        var policy = PolicyBuilder.Build();
        var useCase = CreateUseCase([policy]);

        var result = (await useCase.ListAsync(null, null, FirstPage)).Itens.ShouldHaveSingleItem();

        result.Id.ShouldBe(policy.Id);
        result.SeguradoId.ShouldBe(policy.PolicyholderId);
        result.Descricao.ShouldBe(policy.Description);
        result.ValorCobertura.ShouldBe(policy.CoverageAmount);
        result.ValorPremio.ShouldBe(policy.PremiumAmount);
        result.InicioVigencia.ShouldBe(policy.StartDate);
        result.FimVigencia.ShouldBe(policy.EndDate);
        result.Status.ShouldBe(StatusApolice.Ativa);
        result.CriadoEm.ShouldBe(policy.CreatedAt);
        result.AtualizadoEm.ShouldBe(policy.UpdatedAt);
    }

    [Theory(DisplayName = nameof(Deve_CalcularTotalDePaginas_Quando_Listar))]
    [Trait("Application", "Policies - Listing")]
    [InlineData(0, 0)]
    [InlineData(20, 1)]
    [InlineData(21, 2)]
    public async Task Deve_CalcularTotalDePaginas_Quando_Listar(int totalCount, int expectedPages)
    {
        var useCase = CreateUseCase([], totalCount);

        var result = await useCase.ListAsync(null, null, FirstPage);

        result.Pagina.ShouldBe(1);
        result.TamanhoPagina.ShouldBe(RequestPaginationJson.DefaultPageSize);
        result.TotalItens.ShouldBe(totalCount);
        result.TotalPaginas.ShouldBe(expectedPages);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoAntesDaConsulta_Quando_StatusForInvalido))]
    [Trait("Application", "Policies - Listing")]
    public async Task Deve_LancarValidacaoAntesDaConsulta_Quando_StatusForInvalido()
    {
        var useCase = CreateUseCase();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ListAsync(null, (StatusApolice)999, FirstPage));

        exception.GetErrors().ShouldBe([ResourcesMessageException.INVALID_POLICY_STATUS]);
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacaoAntesDaConsulta_Quando_PaginacaoForInvalida))]
    [Trait("Application", "Policies - Listing")]
    [InlineData(0, 20, true, false)]
    [InlineData(1, 0, false, true)]
    [InlineData(1, 101, false, true)]
    [InlineData(-1, 500, true, true)]
    public async Task Deve_LancarValidacaoAntesDaConsulta_Quando_PaginacaoForInvalida(int page, int pageSize, bool invalidPage, bool invalidPageSize)
    {
        var useCase = CreateUseCase();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ListAsync(null, null, new RequestPaginationJson { Pagina = page, TamanhoPagina = pageSize }));

        exception.GetErrors().Contains(ResourcesMessageException.INVALID_PAGE).ShouldBe(invalidPage);
        exception.GetErrors().Any(error => error.Contains(RequestPaginationJson.MaxPageSize.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal)).ShouldBe(invalidPageSize);
    }

    [Fact(DisplayName = nameof(Deve_MapearApoliceExistente_Quando_BuscarPorId))]
    [Trait("Application", "Policies - Listing")]
    public async Task Deve_MapearApoliceExistente_Quando_BuscarPorId()
    {
        var policy = PolicyBuilder.Build();
        policy.Cancel();
        var useCase = CreateUseCase(policy: policy);

        var result = await useCase.GetByIdAsync(policy.Id);

        result.Id.ShouldBe(policy.Id);
        result.Descricao.ShouldBe(policy.Description);
        result.Status.ShouldBe(StatusApolice.Cancelada);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_BuscarApoliceInexistente))]
    [Trait("Application", "Policies - Listing")]
    public async Task Deve_LancarNaoEncontrado_Quando_BuscarApoliceInexistente()
    {
        var useCase = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(() => useCase.GetByIdAsync(Guid.NewGuid()));
    }

    private static ListPoliciesUseCase CreateUseCase(IEnumerable<Policy>? policies = null, int? totalCount = null, Policy? policy = null)
    {
        var policiesReadOnlyRepositoryBuilder = new PoliciesReadOnlyRepositoryBuilder();

        if (policies is not null)
            policiesReadOnlyRepositoryBuilder.GetPage(policies, totalCount);

        if (policy is not null)
            policiesReadOnlyRepositoryBuilder.GetById(policy);

        return new ListPoliciesUseCase(policiesReadOnlyRepositoryBuilder.Build());
    }
}
