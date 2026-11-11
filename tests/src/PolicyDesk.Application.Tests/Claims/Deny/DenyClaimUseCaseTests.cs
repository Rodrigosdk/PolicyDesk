using PolicyDesk.Domain.Validations;
using Microsoft.Extensions.Logging.Abstractions;
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

        await useCase.ExecuteAsync(claim.Id, new RequestDenyClaimJson { Motivo = "  Not covered  " });

        claim.Status.ShouldBe(ClaimStatus.Denied);
        claim.DenialReason.ShouldBe("Not covered");
        unitOfWork.Verify(work => work.CommitAsync(), Times.Once);
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacaoAntesDaBusca_Quando_MotivoForVazio))]
    [Trait("Application", "Claims - Deny")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Deve_LancarValidacaoAntesDaBusca_Quando_MotivoForVazio(string reason)
    {
        var repository = new ClaimsUpdateOnlyRepositoryBuilder();
        var unitOfWork = UnitOfWorkBuilder.Build();
        var useCase = new DenyClaimUseCase(repository.Build(), unitOfWork.Object, NullLogger<DenyClaimUseCase>.Instance);

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ExecuteAsync(Guid.NewGuid(), new RequestDenyClaimJson { Motivo = reason }));

        repository.Mock.Verify(item => item.GetByIdForUpdateAsync(It.IsAny<Guid>()), Times.Never);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontradoSemConfirmar_Quando_SinistroNaoExistir))]
    [Trait("Application", "Claims - Deny")]
    public async Task Deve_LancarNaoEncontradoSemConfirmar_Quando_SinistroNaoExistir()
    {
        var (useCase, unitOfWork) = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(
            () => useCase.ExecuteAsync(Guid.NewGuid(), new RequestDenyClaimJson { Motivo = "Reason" }));

        unitOfWork.Verify(work => work.CommitAsync(), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoSemConfirmar_Quando_SinistroNaoEstiverEmAnalise))]
    [Trait("Application", "Claims - Deny")]
    public async Task Deve_LancarValidacaoSemConfirmar_Quando_SinistroNaoEstiverEmAnalise()
    {
        var claim = ClaimBuilder.Build();
        var (useCase, unitOfWork) = CreateUseCase(claim);

        await Should.ThrowAsync<DomainRuleException>(
            () => useCase.ExecuteAsync(claim.Id, new RequestDenyClaimJson { Motivo = "Reason" }));

        claim.Status.ShouldBe(ClaimStatus.Pending);
        claim.DenialReason.ShouldBeNull();
        unitOfWork.Verify(work => work.CommitAsync(), Times.Never);
    }

    private static (DenyClaimUseCase UseCase, Mock<IUnitOfWork> UnitOfWork) CreateUseCase(Claim? claim = null)
    {
        var repository = new ClaimsUpdateOnlyRepositoryBuilder();
        if (claim is not null)
            repository.GetByIdForUpdate(claim);
        var unitOfWork = UnitOfWorkBuilder.Build();
        return (new DenyClaimUseCase(repository.Build(), unitOfWork.Object, NullLogger<DenyClaimUseCase>.Instance), unitOfWork);
    }


    [Fact(DisplayName = nameof(Deve_LancarValidacaoAntesDaBusca_Quando_MotivoExcederOLimite))]
    [Trait("Application", "Claims - Deny")]
    public async Task Deve_LancarValidacaoAntesDaBusca_Quando_MotivoExcederOLimite()
    {
        var repository = new ClaimsUpdateOnlyRepositoryBuilder();
        var unitOfWork = UnitOfWorkBuilder.Build();
        var useCase = new DenyClaimUseCase(repository.Build(), unitOfWork.Object, NullLogger<DenyClaimUseCase>.Instance);
        var request = new RequestDenyClaimJson { Motivo = new string('a', FieldLimits.DenialReason + 1) };

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(Guid.NewGuid(), request));

        exception.GetErrors().ShouldBe([FieldLimits.DenialReasonTooLong]);
        repository.Mock.Verify(item => item.GetByIdForUpdateAsync(It.IsAny<Guid>()), Times.Never);
        unitOfWork.Verify(work => work.CommitAsync(), Times.Never);
    }
}
