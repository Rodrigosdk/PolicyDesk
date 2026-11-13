using CommonTestUtilities.Entities;
using CommonTestUtilities.Loggers;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Repositories.Claims;
using PolicyDesk.Application.Claims.Payment;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Claims.Payment;

public class PayClaimUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_RegistrarPagamento_Quando_SinistroEstiverAprovado))]
    [Trait("Application", "Claims - Payment")]
    public async Task Deve_RegistrarPagamento_Quando_SinistroEstiverAprovado()
    {
        var claim = ClaimBuilder.BuildWithStatus(ClaimStatus.Approved);
        var useCase = CreateUseCase(claim);

        await useCase.ExecuteAsync(claim.Id);

        claim.Status.ShouldBe(ClaimStatus.Paid);
        claim.PaidAt.ShouldNotBeNull();
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_SinistroNaoExistir))]
    [Trait("Application", "Claims - Payment")]
    public async Task Deve_LancarNaoEncontrado_Quando_SinistroNaoExistir()
    {
        var useCase = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(() => useCase.ExecuteAsync(Guid.NewGuid()));
    }

    [Fact(DisplayName = nameof(Deve_LancarRegra_Quando_SinistroNaoEstiverAprovado))]
    [Trait("Application", "Claims - Payment")]
    public async Task Deve_LancarRegra_Quando_SinistroNaoEstiverAprovado()
    {
        var claim = ClaimBuilder.BuildWithStatus(ClaimStatus.InAnalysis);
        var useCase = CreateUseCase(claim);

        await Should.ThrowAsync<DomainRuleException>(() => useCase.ExecuteAsync(claim.Id));

        claim.Status.ShouldBe(ClaimStatus.InAnalysis);
        claim.PaidAt.ShouldBeNull();
    }

    private static PayClaimUseCase CreateUseCase(Claim? claim = null)
    {
        var claimsUpdateOnlyRepositoryBuilder = new ClaimsUpdateOnlyRepositoryBuilder();

        if (claim is not null)
            claimsUpdateOnlyRepositoryBuilder.GetByIdForUpdate(claim);

        return new PayClaimUseCase(
            claimsUpdateOnlyRepositoryBuilder.Build(),
            UnitOfWorkBuilder.Build(),
            LoggerBuilder.Build<PayClaimUseCase>());
    }
}
