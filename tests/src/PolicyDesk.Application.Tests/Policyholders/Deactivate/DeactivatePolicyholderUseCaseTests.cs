using Microsoft.Extensions.Logging.Abstractions;
using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Time;
using Moq;
using PolicyDesk.Application.Policyholders.Deactivate;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;
using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Application.Tests.Policyholders.Deactivate;

public class DeactivatePolicyholderUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_InativarSeguradoEConfirmar_Quando_NaoHouverApoliceAtiva))]
    [Trait("Application", "Policyholders - Deactivate")]
    public async Task Deve_InativarSeguradoEConfirmar_Quando_NaoHouverApoliceAtiva()
    {
        var policyholder = PolicyholderBuilder.Build();
        var (useCase, unitOfWork) = CreateUseCase(policyholder);

        await useCase.ExecuteAsync(policyholder.Id);

        policyholder.Active.ShouldBeFalse();
        unitOfWork.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontradoSemConfirmar_Quando_SeguradoNaoExistir))]
    [Trait("Application", "Policyholders - Deactivate")]
    public async Task Deve_LancarNaoEncontradoSemConfirmar_Quando_SeguradoNaoExistir()
    {
        var (useCase, unitOfWork) = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(() => useCase.ExecuteAsync(Guid.NewGuid()));

        unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemInativarNemConfirmar_Quando_HouverApoliceAtiva))]
    [Trait("Application", "Policyholders - Deactivate")]
    public async Task Deve_LancarExcecaoSemInativarNemConfirmar_Quando_HouverApoliceAtiva()
    {
        var policyholder = PolicyholderBuilder.Build();
        var (useCase, unitOfWork) = CreateUseCase(policyholder, hasActivePolicy: true);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ExecuteAsync(policyholder.Id));

        policyholder.Active.ShouldBeTrue();
        exception.GetErrors().ShouldContain(ResourcesMessageException.POLICYHOLDER_HAS_ACTIVE_POLICY);
        unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemInativar_Quando_ApoliceTerminarHojeNoBrasilMasJaForAmanhaEmUtc))]
    [Trait("Application", "Policyholders - Deactivate")]
    public async Task Deve_LancarExcecaoSemInativar_Quando_ApoliceTerminarHojeNoBrasilMasJaForAmanhaEmUtc()
    {
        var policyholder = PolicyholderBuilder.Build();
        var clock = BusinessClockBuilder.Build(BusinessClockBuilder.LateEveningUtcNow);
        var (useCase, unitOfWork) = CreateUseCase(
            policyholder, hasActivePolicy: true, today: BusinessClockBuilder.DefaultToday, clock: clock);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ExecuteAsync(policyholder.Id));

        policyholder.Active.ShouldBeTrue();
        exception.GetErrors().ShouldContain(ResourcesMessageException.POLICYHOLDER_HAS_ACTIVE_POLICY);
        unitOfWork.Verify(u => u.CommitAsync(), Times.Never);
    }

    private static (DeactivatePolicyholderUseCase UseCase, Mock<IUnitOfWork> UnitOfWork) CreateUseCase(
        Policyholder? policyholder = null,
        bool hasActivePolicy = false,
        DateOnly? today = null,
        BusinessClock? clock = null)
    {
        var readBuilder = new PolicyholdersReadOnlyRepositoryBuilder();
        var updateBuilder = new PolicyholdersUpdateOnlyRepositoryBuilder();
        if (policyholder is not null)
        {
            readBuilder.HasActiveOrFuturePolicy(policyholder.Id, hasActivePolicy, today);
            updateBuilder.GetByIdForUpdate(policyholder);
        }

        var unitOfWork = UnitOfWorkBuilder.Build();

        return (new DeactivatePolicyholderUseCase(readBuilder.Build(), updateBuilder.Build(), unitOfWork.Object, clock ?? BusinessClockBuilder.Build(), NullLogger<DeactivatePolicyholderUseCase>.Instance), unitOfWork);
    }
}
