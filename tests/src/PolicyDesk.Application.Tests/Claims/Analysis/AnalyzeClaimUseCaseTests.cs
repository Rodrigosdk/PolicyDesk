using CommonTestUtilities.Entities;
using CommonTestUtilities.Loggers;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Repositories.Claims;
using PolicyDesk.Application.Claims.Analysis;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Claims.Analysis;

public class AnalyzeClaimUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_IniciarAnalise_Quando_SinistroEstiverPendente))]
    [Trait("Application", "Claims - Analysis")]
    public async Task Deve_IniciarAnalise_Quando_SinistroEstiverPendente()
    {
        var claim = ClaimBuilder.Build();
        var useCase = CreateUseCase(claim);

        await useCase.ExecuteAsync(claim.Id);

        claim.Status.ShouldBe(ClaimStatus.InAnalysis);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_SinistroNaoExistir))]
    [Trait("Application", "Claims - Analysis")]
    public async Task Deve_LancarNaoEncontrado_Quando_SinistroNaoExistir()
    {
        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(Guid.NewGuid()).ShouldThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = nameof(Deve_LancarRegra_Quando_SinistroNaoEstiverPendente))]
    [Trait("Application", "Claims - Analysis")]
    public async Task Deve_LancarRegra_Quando_SinistroNaoEstiverPendente()
    {
        var claim = ClaimBuilder.BuildWithStatus(ClaimStatus.InAnalysis);
        var useCase = CreateUseCase(claim);

        await useCase.ExecuteAsync(claim.Id).ShouldThrowAsync<DomainRuleException>();

        claim.Status.ShouldBe(ClaimStatus.InAnalysis);
    }

    private static AnalyzeClaimUseCase CreateUseCase(Claim? claim = null)
    {
        var claimsUpdateOnlyRepositoryBuilder = new ClaimsUpdateOnlyRepositoryBuilder();

        if (claim is not null)
            claimsUpdateOnlyRepositoryBuilder.GetByIdForUpdate(claim);

        return new AnalyzeClaimUseCase(
            claimsUpdateOnlyRepositoryBuilder.Build(),
            UnitOfWorkBuilder.Build(),
            LoggerBuilder.Build<AnalyzeClaimUseCase>());
    }
}
