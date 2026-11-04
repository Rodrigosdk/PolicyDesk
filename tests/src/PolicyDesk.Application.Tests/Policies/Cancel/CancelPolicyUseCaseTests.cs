using Microsoft.Extensions.Logging.Abstractions;
using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using Moq;
using PolicyDesk.Application.Policies.Cancel;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Policies.Cancel;

public class CancelPolicyUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_CancelarApoliceEConfirmar_Quando_ApoliceEstiverAtiva))]
    [Trait("Application", "Policies - Cancel")]
    public async Task Deve_CancelarApoliceEConfirmar_Quando_ApoliceEstiverAtiva()
    {
        var policy = PolicyBuilder.Build();
        var (useCase, unitOfWork) = CreateUseCase(policy);

        await useCase.Execute(policy.Id);

        policy.Status.ShouldBe(PolicyStatus.Cancelled);
        policy.CancelledAt.ShouldNotBeNull();
        unitOfWork.Verify(work => work.Commit(), Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontradoSemConfirmar_Quando_ApoliceNaoExistir))]
    [Trait("Application", "Policies - Cancel")]
    public async Task Deve_LancarNaoEncontradoSemConfirmar_Quando_ApoliceNaoExistir()
    {
        var (useCase, unitOfWork) = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(() => useCase.Execute(Guid.NewGuid()));

        unitOfWork.Verify(work => work.Commit(), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoSemConfirmar_Quando_ApoliceJaEstiverCancelada))]
    [Trait("Application", "Policies - Cancel")]
    public async Task Deve_LancarValidacaoSemConfirmar_Quando_ApoliceJaEstiverCancelada()
    {
        var policy = PolicyBuilder.Build();
        policy.Cancel();
        var (useCase, unitOfWork) = CreateUseCase(policy);

        await Should.ThrowAsync<DomainRuleException>(() => useCase.Execute(policy.Id));

        policy.Status.ShouldBe(PolicyStatus.Cancelled);
        unitOfWork.Verify(work => work.Commit(), Times.Never);
    }

    private static (CancelPolicyUseCase UseCase, Mock<IUnitOfWork> UnitOfWork) CreateUseCase(Policy? policy = null)
    {
        var repository = new PoliciesUpdateOnlyRepositoryBuilder();
        if (policy is not null)
            repository.GetByIdForUpdate(policy);
        var unitOfWork = UnitOfWorkBuilder.Build();
        return (new CancelPolicyUseCase(repository.Build(), unitOfWork.Object, NullLogger<CancelPolicyUseCase>.Instance), unitOfWork);
    }
}
