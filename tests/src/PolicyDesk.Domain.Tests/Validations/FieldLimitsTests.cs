using CommonTestUtilities.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Domain.Tests.Validations;

public class FieldLimitsTests
{
    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemNegar_Quando_MotivoDaNegacaoExcederOLimite))]
    [Trait("Domain", "FieldLimits - Sinistro")]
    public void Deve_LancarExcecaoSemNegar_Quando_MotivoDaNegacaoExcederOLimite()
    {
        var claim = ClaimBuilder.BuildWithStatus(ClaimStatus.InAnalysis);

        var exception = Should.Throw<DomainRuleException>(
            () => claim.Deny(new string('a', FieldLimits.DenialReason + 1)));

        exception.Message.ShouldBe(FieldLimits.DenialReasonTooLong);
        claim.Status.ShouldBe(ClaimStatus.InAnalysis);
        claim.DenialReason.ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_Negar_Quando_MotivoTiverExatamenteOLimite))]
    [Trait("Domain", "FieldLimits - Sinistro")]
    public void Deve_Negar_Quando_MotivoTiverExatamenteOLimite()
    {
        var claim = ClaimBuilder.BuildWithStatus(ClaimStatus.InAnalysis);
        var reason = new string('a', FieldLimits.DenialReason);

        claim.Deny(reason);

        claim.DenialReason.ShouldBe(reason);
    }
}
