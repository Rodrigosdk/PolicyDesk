using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using Moq;
using PolicyDesk.Application.Claims.Deny;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Claims.Deny;

public class DenyClaimUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_NegarEConfirmar_Quando_SinistroEstiverEmAnalise))]
    [Trait("Application", "Claims - Deny")]
    public async Task Deve_NegarEConfirmar_Quando_SinistroEstiverEmAnalise()
    {
        var claim = ClaimBuilder.Build();
        claim.StartAnalysis();
        var (useCase, unitOfWork) = CreateUseCase(claim);

        await useCase.Execute(claim.Id, new RequestDenyClaimJson { Motivo = "  Not covered  " });

        claim.Status.ShouldBe(ClaimStatus.Denied);
        claim.DenialReason.ShouldBe("Not covered");
        unitOfWork.Verify(work => work.Commit(), Times.Once);
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacaoAntesDaBusca_Quando_MotivoForVazio))]
    [Trait("Application", "Claims - Deny")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Deve_LancarValidacaoAntesDaBusca_Quando_MotivoForVazio(string reason)
    {
        var repository = new ClaimsReadOnlyRepositoryBuilder();
        var unitOfWork = UnitOfWorkBuilder.Build();
        var useCase = new DenyClaimUseCase(repository.Build(), unitOfWork.Object);

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.Execute(Guid.NewGuid(), new RequestDenyClaimJson { Motivo = reason }));

        repository.Mock.Verify(item => item.GetById(It.IsAny<Guid>()), Times.Never);
        unitOfWork.Verify(work => work.Commit(), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontradoSemConfirmar_Quando_SinistroNaoExistir))]
    [Trait("Application", "Claims - Deny")]
    public async Task Deve_LancarNaoEncontradoSemConfirmar_Quando_SinistroNaoExistir()
    {
        var (useCase, unitOfWork) = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(
            () => useCase.Execute(Guid.NewGuid(), new RequestDenyClaimJson { Motivo = "Reason" }));

        unitOfWork.Verify(work => work.Commit(), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoSemConfirmar_Quando_SinistroNaoEstiverEmAnalise))]
    [Trait("Application", "Claims - Deny")]
    public async Task Deve_LancarValidacaoSemConfirmar_Quando_SinistroNaoEstiverEmAnalise()
    {
        var claim = ClaimBuilder.Build();
        var (useCase, unitOfWork) = CreateUseCase(claim);

        await Should.ThrowAsync<DomainRuleException>(
            () => useCase.Execute(claim.Id, new RequestDenyClaimJson { Motivo = "Reason" }));

        claim.Status.ShouldBe(ClaimStatus.Pending);
        claim.DenialReason.ShouldBeNull();
        unitOfWork.Verify(work => work.Commit(), Times.Never);
    }

    private static (DenyClaimUseCase UseCase, Mock<IUnitOfWork> UnitOfWork) CreateUseCase(Claim? claim = null)
    {
        var repository = new ClaimsReadOnlyRepositoryBuilder();
        if (claim is not null)
            repository.GetById(claim);
        var unitOfWork = UnitOfWorkBuilder.Build();
        return (new DenyClaimUseCase(repository.Build(), unitOfWork.Object), unitOfWork);
    }
}
