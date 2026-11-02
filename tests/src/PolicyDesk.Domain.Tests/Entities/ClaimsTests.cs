using CommonTestUtilities.Entities;
using PolicyDesk.Domain.Enums;
using Shouldly;

namespace PolicyDesk.Domain.Tests.Entities;

public class ClaimsTests
{
    [Fact(DisplayName = nameof(Deve_InstanciarSinistroPendente_Quando_DadosForemValidos))]
    [Trait("Domain", "Claims - Instancia")]
    public void Deve_InstanciarSinistroPendente_Quando_DadosForemValidos()
    {
        var policyId = Guid.NewGuid();
        var occurredOn = new DateTime(2026, 3, 4, 15, 45, 0, DateTimeKind.Utc);
        var beforeCreation = DateTime.UtcNow;

        var claim = ClaimBuilder.Build(policyId, "  Water damage  ", 1500, occurredOn);

        var afterCreation = DateTime.UtcNow;
        claim.Id.ShouldNotBe(Guid.Empty);
        claim.PolicyId.ShouldBe(policyId);
        claim.Description.ShouldBe("Water damage");
        claim.Amount.ShouldBe(1500);
        claim.OccurredOn.ShouldBe(occurredOn.Date);
        claim.Status.ShouldBe(ClaimStatus.Pending);
        claim.IsApproved.ShouldBeFalse();
        claim.CreatedAt.ShouldBeInRange(beforeCreation, afterCreation);
        claim.UpdatedAt.ShouldBeNull();
        claim.ApprovedAt.ShouldBeNull();
        claim.DenialReason.ShouldBeNull();
        claim.PaidAt.ShouldBeNull();
        claim.Active.ShouldBeTrue();
    }

    [Fact(DisplayName = nameof(Deve_IniciarAnalise_Quando_Solicitado))]
    [Trait("Domain", "Claims - Análise")]
    public void Deve_IniciarAnalise_Quando_Solicitado()
    {
        var claim = ClaimBuilder.Build();

        claim.StartAnalysis();

        claim.Status.ShouldBe(ClaimStatus.InAnalysis);
        claim.UpdatedAt.ShouldNotBeNull();
    }

    [Fact(DisplayName = nameof(Deve_AprovarERegistrarData_Quando_Solicitado))]
    [Trait("Domain", "Claims - Aprovação")]
    public void Deve_AprovarERegistrarData_Quando_Solicitado()
    {
        var claim = ClaimBuilder.Build();
        var before = DateTime.UtcNow;

        claim.Approve();

        var after = DateTime.UtcNow;
        claim.Status.ShouldBe(ClaimStatus.Approved);
        claim.IsApproved.ShouldBeTrue();
        claim.ApprovedAt.ShouldNotBeNull();
        claim.ApprovedAt.Value.ShouldBeInRange(before, after);
        claim.UpdatedAt.ShouldBe(claim.ApprovedAt);
    }

    [Fact(DisplayName = nameof(Deve_NegarERegistrarMotivoSemEspacos_Quando_Solicitado))]
    [Trait("Domain", "Claims - Negação")]
    public void Deve_NegarERegistrarMotivoSemEspacos_Quando_Solicitado()
    {
        var claim = ClaimBuilder.Build();

        claim.Deny("  Not covered  ");

        claim.Status.ShouldBe(ClaimStatus.Denied);
        claim.DenialReason.ShouldBe("Not covered");
        claim.IsApproved.ShouldBeFalse();
        claim.UpdatedAt.ShouldNotBeNull();
    }

    [Fact(DisplayName = nameof(Deve_RegistrarPagamento_Quando_Solicitado))]
    [Trait("Domain", "Claims - Pagamento")]
    public void Deve_RegistrarPagamento_Quando_Solicitado()
    {
        var claim = ClaimBuilder.Build();
        var before = DateTime.UtcNow;

        claim.RegisterPayment();

        var after = DateTime.UtcNow;
        claim.Status.ShouldBe(ClaimStatus.Paid);
        claim.PaidAt.ShouldNotBeNull();
        claim.PaidAt.Value.ShouldBeInRange(before, after);
        claim.UpdatedAt.ShouldBe(claim.PaidAt);
    }
}
