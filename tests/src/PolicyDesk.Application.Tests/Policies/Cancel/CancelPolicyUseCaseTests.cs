using CommonTestUtilities.Entities;
using CommonTestUtilities.Loggers;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Repositories.Policies;
using PolicyDesk.Application.Policies.Cancel;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Policies.Cancel;

public class CancelPolicyUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_CancelarApolice_Quando_ApoliceEstiverAtiva))]
    [Trait("Application", "Policies - Cancel")]
    public async Task Deve_CancelarApolice_Quando_ApoliceEstiverAtiva()
    {
        var policy = PolicyBuilder.Build();
        var useCase = CreateUseCase(policy);

        await useCase.ExecuteAsync(policy.Id);

        policy.Status.ShouldBe(PolicyStatus.Cancelled);
        policy.CancelledAt.ShouldNotBeNull();
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_ApoliceNaoExistir))]
    [Trait("Application", "Policies - Cancel")]
    public async Task Deve_LancarNaoEncontrado_Quando_ApoliceNaoExistir()
    {
        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(Guid.NewGuid()).ShouldThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = nameof(Deve_LancarRegra_Quando_ApoliceJaEstiverCancelada))]
    [Trait("Application", "Policies - Cancel")]
    public async Task Deve_LancarRegra_Quando_ApoliceJaEstiverCancelada()
    {
        var policy = PolicyBuilder.Build();
        policy.Cancel();
        var useCase = CreateUseCase(policy);

        await useCase.ExecuteAsync(policy.Id).ShouldThrowAsync<DomainRuleException>();

        policy.Status.ShouldBe(PolicyStatus.Cancelled);
    }

    private static CancelPolicyUseCase CreateUseCase(Policy? policy = null)
    {
        var policiesUpdateOnlyRepositoryBuilder = new PoliciesUpdateOnlyRepositoryBuilder();

        if (policy is not null)
            policiesUpdateOnlyRepositoryBuilder.GetByIdForUpdate(policy);

        return new CancelPolicyUseCase(
            policiesUpdateOnlyRepositoryBuilder.Build(),
            UnitOfWorkBuilder.Build(),
            LoggerBuilder.Build<CancelPolicyUseCase>());
    }
}
