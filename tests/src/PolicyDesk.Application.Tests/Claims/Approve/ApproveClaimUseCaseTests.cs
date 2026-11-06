using Microsoft.Extensions.Logging.Abstractions;
using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using Moq;
using PolicyDesk.Application.Claims.Approve;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Claims.Approve;

public class ApproveClaimUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_AprovarEConfirmar_Quando_SinistroEstiverEmAnalise))]
    [Trait("Application", "Claims - Approve")]
    public async Task Deve_AprovarEConfirmar_Quando_SinistroEstiverEmAnalise()
    {
        var claim = ClaimBuilder.Build();
        claim.StartAnalysis();
        var (useCase, unitOfWork) = CreateUseCase(claim);

        await useCase.ExecuteAsync(claim.Id);

        claim.Status.ShouldBe(ClaimStatus.Approved);
        claim.ApprovedAt.ShouldNotBeNull();
        unitOfWork.Verify(work => work.CommitAsync(), Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontradoSemConfirmar_Quando_SinistroNaoExistir))]
    [Trait("Application", "Claims - Approve")]
    public async Task Deve_LancarNaoEncontradoSemConfirmar_Quando_SinistroNaoExistir()
    {
        var (useCase, unitOfWork) = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(() => useCase.ExecuteAsync(Guid.NewGuid()));

        unitOfWork.Verify(work => work.CommitAsync(), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoSemConfirmar_Quando_SinistroNaoEstiverEmAnalise))]
    [Trait("Application", "Claims - Approve")]
    public async Task Deve_LancarValidacaoSemConfirmar_Quando_SinistroNaoEstiverEmAnalise()
    {
        var claim = ClaimBuilder.Build();
        var (useCase, unitOfWork) = CreateUseCase(claim);

        await Should.ThrowAsync<DomainRuleException>(() => useCase.ExecuteAsync(claim.Id));

        claim.Status.ShouldBe(ClaimStatus.Pending);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Never);
    }

    private static (ApproveClaimUseCase UseCase, Mock<IUnitOfWork> UnitOfWork) CreateUseCase(Claim? claim = null)
    {
        var repository = new ClaimsUpdateOnlyRepositoryBuilder();
        if (claim is not null)
            repository.GetByIdForUpdate(claim);
        var unitOfWork = UnitOfWorkBuilder.Build();
        return (new ApproveClaimUseCase(repository.Build(), unitOfWork.Object, NullLogger<ApproveClaimUseCase>.Instance), unitOfWork);
    }


    [Fact(DisplayName = nameof(Deve_RepassarOTokenDeCancelamento_Quando_BuscarEConfirmar))]
    [Trait("Application", "Claims - Approve")]
    public async Task Deve_RepassarOTokenDeCancelamento_Quando_BuscarEConfirmar()
    {
        var claim = ClaimBuilder.BuildWithStatus(ClaimStatus.InAnalysis);
        var repository = new ClaimsUpdateOnlyRepositoryBuilder().GetByIdForUpdate(claim);
        var unitOfWork = UnitOfWorkBuilder.Build();
        var useCase = new ApproveClaimUseCase(repository.Build(), unitOfWork.Object, NullLogger<ApproveClaimUseCase>.Instance);
        using var cancellation = new CancellationTokenSource();

        await useCase.ExecuteAsync(claim.Id, cancellation.Token);

        repository.Mock.Verify(item => item.GetByIdForUpdateAsync(claim.Id, cancellation.Token), Times.Once);
        unitOfWork.Verify(work => work.CommitAsync(cancellation.Token), Times.Once);
    }
}
