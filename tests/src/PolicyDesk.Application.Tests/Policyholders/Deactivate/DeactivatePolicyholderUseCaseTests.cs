using CommonTestUtilities.Entities;
using CommonTestUtilities.Loggers;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Repositories.Policyholders;
using CommonTestUtilities.Time;
using PolicyDesk.Application.Policyholders.Deactivate;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Policyholders.Deactivate;

public class DeactivatePolicyholderUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_InativarSegurado_Quando_NaoHouverApoliceAtiva))]
    [Trait("Application", "Policyholders - Deactivate")]
    public async Task Deve_InativarSegurado_Quando_NaoHouverApoliceAtiva()
    {
        var policyholder = PolicyholderBuilder.Build();
        var useCase = CreateUseCase(policyholder);

        await useCase.ExecuteAsync(policyholder.Id);

        policyholder.Active.ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_SeguradoNaoExistir))]
    [Trait("Application", "Policyholders - Deactivate")]
    public async Task Deve_LancarNaoEncontrado_Quando_SeguradoNaoExistir()
    {
        var useCase = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(() => useCase.ExecuteAsync(Guid.NewGuid()));
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemInativar_Quando_HouverApoliceAtiva))]
    [Trait("Application", "Policyholders - Deactivate")]
    public async Task Deve_LancarExcecaoSemInativar_Quando_HouverApoliceAtiva()
    {
        var policyholder = PolicyholderBuilder.Build();
        var useCase = CreateUseCase(policyholder, hasActivePolicy: true);

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(policyholder.Id));

        policyholder.Active.ShouldBeTrue();
        exception.GetErrors().ShouldContain(ResourcesMessageException.POLICYHOLDER_HAS_ACTIVE_POLICY);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemInativar_Quando_ApoliceTerminarHojeNoBrasilMasJaForAmanhaEmUtc))]
    [Trait("Application", "Policyholders - Deactivate")]
    public async Task Deve_LancarExcecaoSemInativar_Quando_ApoliceTerminarHojeNoBrasilMasJaForAmanhaEmUtc()
    {
        var policyholder = PolicyholderBuilder.Build();
        var useCase = CreateUseCase(
            policyholder,
            hasActivePolicy: true,
            today: BusinessClockBuilder.DefaultToday,
            clock: BusinessClockBuilder.Build(BusinessClockBuilder.LateEveningUtcNow));

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(policyholder.Id));

        policyholder.Active.ShouldBeTrue();
        exception.GetErrors().ShouldContain(ResourcesMessageException.POLICYHOLDER_HAS_ACTIVE_POLICY);
    }

    private static DeactivatePolicyholderUseCase CreateUseCase(
        Policyholder? policyholder = null,
        bool hasActivePolicy = false,
        DateOnly? today = null,
        BusinessClock? clock = null)
    {
        var policyholdersReadOnlyRepositoryBuilder = new PolicyholdersReadOnlyRepositoryBuilder();
        var policyholdersUpdateOnlyRepositoryBuilder = new PolicyholdersUpdateOnlyRepositoryBuilder();

        if (policyholder is not null)
        {
            policyholdersReadOnlyRepositoryBuilder.HasActiveOrFuturePolicy(policyholder.Id, hasActivePolicy, today);
            policyholdersUpdateOnlyRepositoryBuilder.GetByIdForUpdate(policyholder);
        }

        return new DeactivatePolicyholderUseCase(
            policyholdersReadOnlyRepositoryBuilder.Build(),
            policyholdersUpdateOnlyRepositoryBuilder.Build(),
            UnitOfWorkBuilder.Build(),
            clock ?? BusinessClockBuilder.Build(),
            LoggerBuilder.Build<DeactivatePolicyholderUseCase>());
    }
}
