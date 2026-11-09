using Microsoft.Extensions.Logging.Abstractions;
using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using Moq;
using PolicyDesk.Application.Claims.Payment;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Claims.Payment;

public class PayClaimUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_RegistrarPagamentoEConfirmar_Quando_SinistroEstiverAprovado))]
    [Trait("Application", "Claims - Payment")]
    public async Task Deve_RegistrarPagamentoEConfirmar_Quando_SinistroEstiverAprovado()
    {
        var claim = ClaimBuilder.Build();
        claim.StartAnalysis();
        claim.Approve();
        var (useCase, unitOfWork) = CreateUseCase(claim);

        await useCase.ExecuteAsync(claim.Id);

        claim.Status.ShouldBe(ClaimStatus.Paid);
        claim.PaidAt.ShouldNotBeNull();
        unitOfWork.Verify(work => work.CommitAsync(), Times.Once);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontradoSemConfirmar_Quando_SinistroNaoExistir))]
    [Trait("Application", "Claims - Payment")]
    public async Task Deve_LancarNaoEncontradoSemConfirmar_Quando_SinistroNaoExistir()
    {
        var (useCase, unitOfWork) = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(() => useCase.ExecuteAsync(Guid.NewGuid()));

        unitOfWork.Verify(work => work.CommitAsync(), Times.Never);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoSemConfirmar_Quando_SinistroNaoEstiverAprovado))]
    [Trait("Application", "Claims - Payment")]
    public async Task Deve_LancarValidacaoSemConfirmar_Quando_SinistroNaoEstiverAprovado()
    {
        var claim = ClaimBuilder.Build();
        claim.StartAnalysis();
        var (useCase, unitOfWork) = CreateUseCase(claim);

        await Should.ThrowAsync<DomainRuleException>(() => useCase.ExecuteAsync(claim.Id));

        claim.Status.ShouldBe(ClaimStatus.InAnalysis);
        claim.PaidAt.ShouldBeNull();
        unitOfWork.Verify(work => work.CommitAsync(), Times.Never);
    }

    private static (PayClaimUseCase UseCase, Mock<IUnitOfWork> UnitOfWork) CreateUseCase(Claim? claim = null)
    {
        var repository = new ClaimsUpdateOnlyRepositoryBuilder();
        if (claim is not null)
            repository.GetByIdForUpdate(claim);
        var unitOfWork = UnitOfWorkBuilder.Build();
        return (new PayClaimUseCase(repository.Build(), unitOfWork.Object, NullLogger<PayClaimUseCase>.Instance), unitOfWork);
    }
}
