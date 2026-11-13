using CommonTestUtilities.Entities;
using CommonTestUtilities.Loggers;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Repositories.Claims;
using PolicyDesk.Application.Claims.Deny;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Application.Tests.Claims.Deny;

public class DenyClaimUseCaseTests
{
    [Fact(DisplayName = nameof(Deve_Negar_Quando_SinistroEstiverEmAnalise))]
    [Trait("Application", "Claims - Deny")]
    public async Task Deve_Negar_Quando_SinistroEstiverEmAnalise()
    {
        var claim = ClaimBuilder.BuildWithStatus(ClaimStatus.InAnalysis);
        var useCase = CreateUseCase(claim);

        await useCase.ExecuteAsync(claim.Id, new RequestDenyClaimJson { Motivo = "  Not covered  " });

        claim.Status.ShouldBe(ClaimStatus.Denied);
        claim.DenialReason.ShouldBe("Not covered");
    }

    [Theory(DisplayName = nameof(Deve_LancarValidacaoAntesDaBusca_Quando_MotivoForVazio))]
    [Trait("Application", "Claims - Deny")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Deve_LancarValidacaoAntesDaBusca_Quando_MotivoForVazio(string reason)
    {
        var useCase = CreateUseCase();

        await Should.ThrowAsync<ErrorOnValidationException>(
            () => useCase.ExecuteAsync(Guid.NewGuid(), new RequestDenyClaimJson { Motivo = reason }));
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacaoAntesDaBusca_Quando_MotivoExcederOLimite))]
    [Trait("Application", "Claims - Deny")]
    public async Task Deve_LancarValidacaoAntesDaBusca_Quando_MotivoExcederOLimite()
    {
        var request = new RequestDenyClaimJson { Motivo = new string('a', FieldLimits.DenialReason + 1) };
        var useCase = CreateUseCase();

        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => useCase.ExecuteAsync(Guid.NewGuid(), request));

        exception.GetErrors().ShouldBe([FieldLimits.DenialReasonTooLong]);
    }

    [Fact(DisplayName = nameof(Deve_LancarNaoEncontrado_Quando_SinistroNaoExistir))]
    [Trait("Application", "Claims - Deny")]
    public async Task Deve_LancarNaoEncontrado_Quando_SinistroNaoExistir()
    {
        var useCase = CreateUseCase();

        await Should.ThrowAsync<NotFoundException>(
            () => useCase.ExecuteAsync(Guid.NewGuid(), new RequestDenyClaimJson { Motivo = "Reason" }));
    }

    [Fact(DisplayName = nameof(Deve_LancarRegra_Quando_SinistroNaoEstiverEmAnalise))]
    [Trait("Application", "Claims - Deny")]
    public async Task Deve_LancarRegra_Quando_SinistroNaoEstiverEmAnalise()
    {
        var claim = ClaimBuilder.Build();
        var useCase = CreateUseCase(claim);

        await Should.ThrowAsync<DomainRuleException>(
            () => useCase.ExecuteAsync(claim.Id, new RequestDenyClaimJson { Motivo = "Reason" }));

        claim.Status.ShouldBe(ClaimStatus.Pending);
        claim.DenialReason.ShouldBeNull();
    }

    private static DenyClaimUseCase CreateUseCase(Claim? claim = null)
    {
        var claimsUpdateOnlyRepositoryBuilder = new ClaimsUpdateOnlyRepositoryBuilder();

        if (claim is not null)
            claimsUpdateOnlyRepositoryBuilder.GetByIdForUpdate(claim);

        return new DenyClaimUseCase(
            claimsUpdateOnlyRepositoryBuilder.Build(),
            UnitOfWorkBuilder.Build(),
            LoggerBuilder.Build<DenyClaimUseCase>());
    }
}
