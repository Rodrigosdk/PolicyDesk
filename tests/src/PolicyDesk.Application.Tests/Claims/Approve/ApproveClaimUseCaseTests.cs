using CommonTestUtilities.Entities;
using CommonTestUtilities.Loggers;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Repositories.Claims;
using PolicyDesk.Application.Claims.Approve;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Claims.Approve;

public class ApproveClaimUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_Aprovar_Quando_SinistroEstiverEmAnalise))]
    [Trait("Application", "Claims - Approve")]
    public async Task Deve_Aprovar_Quando_SinistroEstiverEmAnalise()
    {
        var claim = ClaimBuilder.BuildWithStatus(ClaimStatus.InAnalysis);
        var useCase = CreateUseCase(claim);

        await useCase.ExecuteAsync(claim.Id);

        claim.Status.ShouldBe(ClaimStatus.Approved);
        claim.ApprovedAt.ShouldNotBeNull();
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_SinistroNaoExistir))]
    [Trait("Application", "Claims - Approve")]
    public async Task Deve_LancarNaoEncontrado_Quando_SinistroNaoExistir()
    {
        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(Guid.NewGuid()).ShouldThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = nameof(Deve_LancarRegra_Quando_SinistroNaoEstiverEmAnalise))]
    [Trait("Application", "Claims - Approve")]
    public async Task Deve_LancarRegra_Quando_SinistroNaoEstiverEmAnalise()
    {
        var claim = ClaimBuilder.Build();
        var useCase = CreateUseCase(claim);

        await useCase.ExecuteAsync(claim.Id).ShouldThrowAsync<DomainRuleException>();

        claim.Status.ShouldBe(ClaimStatus.Pending);
    }

    private static ApproveClaimUseCase CreateUseCase(Claim? claim = null)
    {
        var claimsUpdateOnlyRepositoryBuilder = new ClaimsUpdateOnlyRepositoryBuilder();

        if (claim is not null)
            claimsUpdateOnlyRepositoryBuilder.GetByIdForUpdate(claim);

        return new ApproveClaimUseCase(
            claimsUpdateOnlyRepositoryBuilder.Build(),
            UnitOfWorkBuilder.Build(),
            LoggerBuilder.Build<ApproveClaimUseCase>());
    }
}
