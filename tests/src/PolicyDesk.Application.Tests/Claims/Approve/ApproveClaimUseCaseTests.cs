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

        await useCase.Execute(claim.Id);

        claim.Status.ShouldBe(ClaimStatus.Approved);
        claim.ApprovedAt.ShouldNotBeNull();
        unitOfWork.Verify(work => work.Commit(), Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontradoSemConfirmar_Quando_SinistroNaoExistir))]
    [Trait("Application", "Claims - Approve")]
    public async Task Deve_LancarNaoEncontradoSemConfirmar_Quando_SinistroNaoExistir()
    {
        var (useCase, unitOfWork) = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(() => useCase.Execute(Guid.NewGuid()));

        unitOfWork.Verify(work => work.Commit(), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoSemConfirmar_Quando_SinistroNaoEstiverEmAnalise))]
    [Trait("Application", "Claims - Approve")]
    public async Task Deve_LancarValidacaoSemConfirmar_Quando_SinistroNaoEstiverEmAnalise()
    {
        var claim = ClaimBuilder.Build();
        var (useCase, unitOfWork) = CreateUseCase(claim);

        await Should.ThrowAsync<DomainRuleException>(() => useCase.Execute(claim.Id));

        claim.Status.ShouldBe(ClaimStatus.Pending);
        unitOfWork.Verify(work => work.Commit(), Times.Never);
    }

    private static (ApproveClaimUseCase UseCase, Mock<IUnitOfWork> UnitOfWork) CreateUseCase(Claim? claim = null)
    {
        var repository = new ClaimsUpdateOnlyRepositoryBuilder();
        if (claim is not null)
            repository.GetByIdForUpdate(claim);
        var unitOfWork = UnitOfWorkBuilder.Build();
        return (new ApproveClaimUseCase(repository.Build(), unitOfWork.Object, NullLogger<ApproveClaimUseCase>.Instance), unitOfWork);
    }
}
