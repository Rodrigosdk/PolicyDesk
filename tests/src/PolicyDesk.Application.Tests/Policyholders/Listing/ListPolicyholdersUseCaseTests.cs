using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using Moq;
using PolicyDesk.Application.Policyholders.Listing;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Policyholders.Listing;

public class ListPolicyholdersUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_MapearSeguradosParaResposta_Quando_Listar))]
    [Trait("Application", "Policyholders - Listing")]
    public async Task Deve_MapearSeguradosParaResposta_Quando_Listar()
    {
        var policyholder = PolicyholderBuilder.Build();
        var useCase = CreateUseCase(new PolicyholdersReadOnlyRepositoryBuilder().GetAll([policyholder]));

        var result = (await useCase.Listing()).ShouldHaveSingleItem();

        result.Id.ShouldBe(policyholder.Id);
        result.Name.ShouldBe(policyholder.Name);
        result.Cpf.ShouldBe(policyholder.Cpf);
        result.BirthDate.ShouldBe(policyholder.BirthDate);
        result.Email.ShouldBe(policyholder.Email);
        result.Phone.ShouldBe(policyholder.Phone);
        result.CreatedAt.ShouldBe(policyholder.CreatedAt);
        result.UpdatedAt.ShouldBe(policyholder.UpdatedAt);
    }

    [Fact(DisplayName = nameof(Deve_MapearSeguradoExistente_Quando_BuscarPorId))]
    [Trait("Application", "Policyholders - Listing")]
    public async Task Deve_MapearSeguradoExistente_Quando_BuscarPorId()
    {
        var policyholder = PolicyholderBuilder.Build();
        var useCase = CreateUseCase(new PolicyholdersReadOnlyRepositoryBuilder().GetById(policyholder));

        var result = await useCase.GetById(policyholder.Id);

        result.Id.ShouldBe(policyholder.Id);
        result.Email.ShouldBe(policyholder.Email);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_BuscarPorIdInexistente))]
    [Trait("Application", "Policyholders - Listing")]
    public async Task Deve_LancarNaoEncontrado_Quando_BuscarPorIdInexistente()
    {
        var useCase = CreateUseCase(new PolicyholdersReadOnlyRepositoryBuilder());

        await Should.ThrowAsync<NotFoundException>(() => useCase.GetById(Guid.NewGuid()));
    }

    [Fact(DisplayName = nameof(Deve_PropagarExcecaoOriginal_Quando_RepositorioFalhar))]
    [Trait("Application", "Policyholders - Listing")]
    public async Task Deve_PropagarExcecaoOriginal_Quando_RepositorioFalhar()
    {
        // Com ContinueWith a falha chegava embrulhada em AggregateException e virava um 500 genérico
        var readBuilder = new PolicyholdersReadOnlyRepositoryBuilder();
        readBuilder.Mock.Setup(r => r.GetAll()).ThrowsAsync(new TimeoutException("db timeout"));
        var useCase = CreateUseCase(readBuilder);

        var exception = await Should.ThrowAsync<TimeoutException>(() => useCase.Listing());

        exception.Message.ShouldBe("db timeout");
    }

    private static ListPolicyholdersUseCase CreateUseCase(PolicyholdersReadOnlyRepositoryBuilder readBuilder) =>
        new(readBuilder.Build());
}
