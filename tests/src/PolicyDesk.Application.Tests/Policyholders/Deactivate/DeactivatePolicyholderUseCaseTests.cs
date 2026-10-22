using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using Moq;
using PolicyDesk.Application.Policyholders.Deactivate;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;
using Policyholder = PolicyDesk.Domain.Entities.Policyholders;

namespace PolicyDesk.Application.Tests.Policyholders.Deactivate;

public class DeactivatePolicyholderUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_InativarSeguradoEConfirmar_Quando_NaoHouverApoliceAtiva))]
    [Trait("Application", "Policyholders - Deactivate")]
    public async Task Deve_InativarSeguradoEConfirmar_Quando_NaoHouverApoliceAtiva()
    {
        var policyholder = PolicyholderBuilder.Build();
        var (useCase, unitOfWork) = CreateUseCase(policyholder);

        await useCase.Execute(policyholder.Id);

        policyholder.Active.ShouldBeFalse();
        unitOfWork.Verify(u => u.Commit(), Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontradoSemConfirmar_Quando_SeguradoNaoExistir))]
    [Trait("Application", "Policyholders - Deactivate")]
    public async Task Deve_LancarNaoEncontradoSemConfirmar_Quando_SeguradoNaoExistir()
    {
        var (useCase, unitOfWork) = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(() => useCase.Execute(Guid.NewGuid()));

        unitOfWork.Verify(u => u.Commit(), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemInativarNemConfirmar_Quando_HouverApoliceAtiva))]
    [Trait("Application", "Policyholders - Deactivate")]
    public async Task Deve_LancarExcecaoSemInativarNemConfirmar_Quando_HouverApoliceAtiva()
    {
        var policyholder = PolicyholderBuilder.Build();
        var (useCase, unitOfWork) = CreateUseCase(policyholder, hasActivePolicy: true);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.Execute(policyholder.Id));

        policyholder.Active.ShouldBeTrue();
        exception.GetErrors().ShouldContain(ResourcesMessageException.POLICYHOLDER_HAS_ACTIVE_POLICY);
        unitOfWork.Verify(u => u.Commit(), Times.Never);
    }

    private static (DeactivatePolicyholderUseCase UseCase, Mock<IUnitOfWork> UnitOfWork) CreateUseCase(
        Policyholder? policyholder = null,
        bool hasActivePolicy = false)
    {
        var readBuilder = new PolicyholdersReadOnlyRepositoryBuilder();
        if (policyholder is not null)
            readBuilder.GetById(policyholder).HasActivePolicy(policyholder.Id, hasActivePolicy);

        var unitOfWork = UnitOfWorkBuilder.Build();

        return (new DeactivatePolicyholderUseCase(readBuilder.Build(), unitOfWork.Object), unitOfWork);
    }
}
