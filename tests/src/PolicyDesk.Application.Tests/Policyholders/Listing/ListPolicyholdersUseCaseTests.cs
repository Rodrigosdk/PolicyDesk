using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories.Policyholders;
using PolicyDesk.Application.Policyholders.Listing;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Policyholders.Listing;

public class ListPolicyholdersUseCaseTests
{
    private static readonly RequestPaginationJson FirstPage = new();

    [Fact(DisplayName = nameof(Deve_MapearSeguradosParaResposta_Quando_Listar))]
    [Trait("Application", "Policyholders - Listing")]
    public async Task Deve_MapearSeguradosParaResposta_Quando_Listar()
    {
        var policyholder = PolicyholderBuilder.Build();
        var useCase = CreateUseCase(policyholders: [policyholder]);

        var page = await useCase.ListAsync(FirstPage);

        page.TotalItens.ShouldBe(1);
        page.TotalPaginas.ShouldBe(1);
        var result = page.Itens.ShouldHaveSingleItem();
        result.Id.ShouldBe(policyholder.Id);
        result.Nome.ShouldBe(policyholder.Name);
        result.Cpf.ShouldBe(policyholder.Cpf);
        result.DataNascimento.ShouldBe(policyholder.BirthDate);
        result.Email.ShouldBe(policyholder.Email);
        result.Telefone.ShouldBe(policyholder.Phone);
        result.CriadoEm.ShouldBe(policyholder.CreatedAt);
        result.AtualizadoEm.ShouldBe(policyholder.UpdatedAt);
    }

    [Fact(DisplayName = nameof(Deve_CalcularPaginacao_Quando_Listar))]
    [Trait("Application", "Policyholders - Listing")]
    public async Task Deve_CalcularPaginacao_Quando_Listar()
    {
        var useCase = CreateUseCase(policyholders: [], totalCount: 45);

        var page = await useCase.ListAsync(new RequestPaginationJson { Pagina = 2, TamanhoPagina = 10 });

        page.Pagina.ShouldBe(2);
        page.TamanhoPagina.ShouldBe(10);
        page.TotalPaginas.ShouldBe(5);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoAntesDaConsulta_Quando_PaginaForInvalida))]
    [Trait("Application", "Policyholders - Listing")]
    public async Task Deve_LancarValidacaoAntesDaConsulta_Quando_PaginaForInvalida()
    {
        var useCase = CreateUseCase();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ListAsync(new RequestPaginationJson { Pagina = 0 }));

        exception.GetErrors().ShouldBe([ResourcesMessageException.INVALID_PAGE]);
    }

    [Fact(DisplayName = nameof(Deve_MapearSeguradoExistente_Quando_BuscarPorId))]
    [Trait("Application", "Policyholders - Listing")]
    public async Task Deve_MapearSeguradoExistente_Quando_BuscarPorId()
    {
        var policyholder = PolicyholderBuilder.Build();
        var useCase = CreateUseCase(policyholder: policyholder);

        var result = await useCase.GetByIdAsync(policyholder.Id);

        result.Id.ShouldBe(policyholder.Id);
        result.Email.ShouldBe(policyholder.Email);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_BuscarPorIdInexistente))]
    [Trait("Application", "Policyholders - Listing")]
    public async Task Deve_LancarNaoEncontrado_Quando_BuscarPorIdInexistente()
    {
        var useCase = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(() => useCase.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact(DisplayName = nameof(Deve_PropagarExcecaoOriginal_Quando_RepositorioFalhar))]
    [Trait("Application", "Policyholders - Listing")]
    public async Task Deve_PropagarExcecaoOriginal_Quando_RepositorioFalhar()
    {
        var useCase = CreateUseCase(failure: new TimeoutException("db timeout"));

        var exception = await Should.ThrowAsync<TimeoutException>(() => useCase.ListAsync(FirstPage));

        exception.Message.ShouldBe("db timeout");
    }

    private static ListPolicyholdersUseCase CreateUseCase(
        IEnumerable<Policyholder>? policyholders = null,
        int? totalCount = null,
        Policyholder? policyholder = null,
        System.Exception? failure = null)
    {
        var policyholdersReadOnlyRepositoryBuilder = new PolicyholdersReadOnlyRepositoryBuilder();

        if (policyholders is not null)
            policyholdersReadOnlyRepositoryBuilder.GetPage(policyholders, totalCount);

        if (policyholder is not null)
            policyholdersReadOnlyRepositoryBuilder.GetById(policyholder);

        if (failure is not null)
            policyholdersReadOnlyRepositoryBuilder.GetPageThrows(failure);

        return new ListPolicyholdersUseCase(policyholdersReadOnlyRepositoryBuilder.Build());
    }
}
