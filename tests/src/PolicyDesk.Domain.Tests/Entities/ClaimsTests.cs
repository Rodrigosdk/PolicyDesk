using CommonTestUtilities.Entities;
using CommonTestUtilities.Time;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Domain.Tests.Entities;

public class ClaimsTests
{
    [Fact(DisplayName = nameof(Deve_InstanciarSinistroPendente_Quando_DadosForemValidos))]
    [Trait("Domain", "Claims - Instancia")]
    public void Deve_InstanciarSinistroPendente_Quando_DadosForemValidos()
    {
        var policyId = Guid.NewGuid();
        var occurredOn = new DateOnly(2026, 3, 4);
        var beforeCreation = DateTime.UtcNow;

        var claim = Claim.Create(policyId, "  Water damage  ", 1500, occurredOn);

        var afterCreation = DateTime.UtcNow;
        claim.Id.ShouldNotBe(Guid.Empty);
        claim.PolicyId.ShouldBe(policyId);
        claim.Description.ShouldBe("Water damage");
        claim.Amount.ShouldBe(1500);
        claim.OccurredOn.ShouldBe(occurredOn);
        claim.Status.ShouldBe(ClaimStatus.Pending);
        claim.CreatedAt.ShouldBeInRange(beforeCreation, afterCreation);
        claim.UpdatedAt.ShouldBeNull();
        claim.ApprovedAt.ShouldBeNull();
        claim.DenialReason.ShouldBeNull();
        claim.PaidAt.ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_IniciarAnalise_Quando_SinistroEstiverPendente))]
    [Trait("Domain", "Claims - Análise")]
    public void Deve_IniciarAnalise_Quando_SinistroEstiverPendente()
    {
        var claim = ClaimBuilder.Build();

        claim.StartAnalysis();

        claim.Status.ShouldBe(ClaimStatus.InAnalysis);
        claim.UpdatedAt.ShouldNotBeNull();
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoSemAlterar_Quando_IniciarAnaliseDeSinistroNaoPendente))]
    [Trait("Domain", "Claims - Análise")]
    [InlineData(ClaimStatus.InAnalysis)]
    [InlineData(ClaimStatus.Approved)]
    [InlineData(ClaimStatus.Denied)]
    [InlineData(ClaimStatus.Paid)]
    public void Deve_LancarExcecaoSemAlterar_Quando_IniciarAnaliseDeSinistroNaoPendente(ClaimStatus status)
    {
        ShouldRejectTransition(
            status,
            claim => claim.StartAnalysis(),
            ResourcesMessageException.CLAIM_MUST_BE_PENDING_FOR_ANALYSIS);
    }

    [Fact(DisplayName = nameof(Deve_AprovarERegistrarData_Quando_SinistroEstiverEmAnalise))]
    [Trait("Domain", "Claims - Aprovação")]
    public void Deve_AprovarERegistrarData_Quando_SinistroEstiverEmAnalise()
    {
        var claim = ClaimBuilder.BuildWithStatus(ClaimStatus.InAnalysis);
        var before = DateTime.UtcNow;

        claim.Approve();

        var after = DateTime.UtcNow;
        claim.Status.ShouldBe(ClaimStatus.Approved);
        claim.ApprovedAt.ShouldNotBeNull().ShouldBeInRange(before, after);
        claim.UpdatedAt.ShouldBe(claim.ApprovedAt);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoSemAlterar_Quando_AprovarSinistroForaDeAnalise))]
    [Trait("Domain", "Claims - Aprovação")]
    [InlineData(ClaimStatus.Pending)]
    [InlineData(ClaimStatus.Approved)]
    [InlineData(ClaimStatus.Denied)]
    [InlineData(ClaimStatus.Paid)]
    public void Deve_LancarExcecaoSemAlterar_Quando_AprovarSinistroForaDeAnalise(ClaimStatus status)
    {
        ShouldRejectTransition(
            status,
            claim => claim.Approve(),
            ResourcesMessageException.CLAIM_MUST_BE_IN_ANALYSIS_FOR_APPROVAL);
    }

    [Fact(DisplayName = nameof(Deve_NegarERegistrarMotivoSemEspacos_Quando_SinistroEstiverEmAnalise))]
    [Trait("Domain", "Claims - Negação")]
    public void Deve_NegarERegistrarMotivoSemEspacos_Quando_SinistroEstiverEmAnalise()
    {
        var claim = ClaimBuilder.BuildWithStatus(ClaimStatus.InAnalysis);

        claim.Deny("  Not covered  ");

        claim.Status.ShouldBe(ClaimStatus.Denied);
        claim.DenialReason.ShouldBe("Not covered");
        claim.UpdatedAt.ShouldNotBeNull();
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoSemAlterar_Quando_NegarSinistroForaDeAnalise))]
    [Trait("Domain", "Claims - Negação")]
    [InlineData(ClaimStatus.Pending)]
    [InlineData(ClaimStatus.Approved)]
    [InlineData(ClaimStatus.Denied)]
    [InlineData(ClaimStatus.Paid)]
    public void Deve_LancarExcecaoSemAlterar_Quando_NegarSinistroForaDeAnalise(ClaimStatus status)
    {
        ShouldRejectTransition(
            status,
            claim => claim.Deny("Reason"),
            ResourcesMessageException.CLAIM_MUST_BE_IN_ANALYSIS_FOR_DENIAL);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoSemAlterar_Quando_NegarSemMotivo))]
    [Trait("Domain", "Claims - Negação")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_LancarExcecaoSemAlterar_Quando_NegarSemMotivo(string? reason)
    {
        ShouldRejectTransition(
            ClaimStatus.InAnalysis,
            claim => claim.Deny(reason!),
            ResourcesMessageException.CLAIM_DENIAL_REASON_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_RegistrarPagamento_Quando_SinistroEstiverAprovado))]
    [Trait("Domain", "Claims - Pagamento")]
    public void Deve_RegistrarPagamento_Quando_SinistroEstiverAprovado()
    {
        var claim = ClaimBuilder.BuildWithStatus(ClaimStatus.Approved);
        var before = DateTime.UtcNow;

        claim.RegisterPayment();

        var after = DateTime.UtcNow;
        claim.Status.ShouldBe(ClaimStatus.Paid);
        claim.PaidAt.ShouldNotBeNull().ShouldBeInRange(before, after);
        claim.UpdatedAt.ShouldBe(claim.PaidAt);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoSemAlterar_Quando_PagarSinistroNaoAprovado))]
    [Trait("Domain", "Claims - Pagamento")]
    [InlineData(ClaimStatus.Pending)]
    [InlineData(ClaimStatus.InAnalysis)]
    [InlineData(ClaimStatus.Denied)]
    [InlineData(ClaimStatus.Paid)]
    public void Deve_LancarExcecaoSemAlterar_Quando_PagarSinistroNaoAprovado(ClaimStatus status)
    {
        ShouldRejectTransition(
            status,
            claim => claim.RegisterPayment(),
            ResourcesMessageException.CLAIM_MUST_BE_APPROVED_FOR_PAYMENT);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_CriarComDescricaoVazia))]
    [Trait("Domain", "Claims - Instancia")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_LancarExcecao_Quando_CriarComDescricaoVazia(string? description)
    {
        var exception = Should.Throw<DomainRuleException>(
            () => Claim.Create(Guid.NewGuid(), description!, 100, BusinessClockBuilder.DefaultToday));

        exception.Message.ShouldBe(ResourcesMessageException.DESCRIPTION_REQUIRED);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_CriarComValorNaoPositivo))]
    [Trait("Domain", "Claims - Instancia")]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Deve_LancarExcecao_Quando_CriarComValorNaoPositivo(decimal amount)
    {
        var exception = Should.Throw<DomainRuleException>(
            () => Claim.Create(Guid.NewGuid(), "Description", amount, BusinessClockBuilder.DefaultToday));

        exception.Message.ShouldBe(ResourcesMessageException.CLAIM_AMOUNT_MUST_BE_POSITIVE);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_CriarSemApolice))]
    [Trait("Domain", "Claims - Instancia")]
    public void Deve_LancarExcecao_Quando_CriarSemApolice()
    {
        var exception = Should.Throw<DomainRuleException>(
            () => Claim.Create(Guid.Empty, "Description", 100, BusinessClockBuilder.DefaultToday));

        exception.Message.ShouldBe(ResourcesMessageException.POLICY_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_CriarSemDataDeOcorrencia))]
    [Trait("Domain", "Claims - Instancia")]
    public void Deve_LancarExcecao_Quando_CriarSemDataDeOcorrencia()
    {
        var exception = Should.Throw<DomainRuleException>(
            () => Claim.Create(Guid.NewGuid(), "Description", 100, default));

        exception.Message.ShouldBe(ResourcesMessageException.CLAIM_OCCURRENCE_DATE_REQUIRED);
    }

    // Verifica que a transição é rejeitada com a mensagem esperada e que nenhum campo foi alterado
    private static void ShouldRejectTransition(ClaimStatus status, Action<Claim> transition, string expectedError)
    {
        var claim = ClaimBuilder.BuildWithStatus(status);
        var updatedAt = claim.UpdatedAt;
        var approvedAt = claim.ApprovedAt;
        var denialReason = claim.DenialReason;
        var paidAt = claim.PaidAt;

        var exception = Should.Throw<DomainRuleException>(() => transition(claim));

        exception.Message.ShouldBe(expectedError);
        claim.Status.ShouldBe(status);
        claim.UpdatedAt.ShouldBe(updatedAt);
        claim.ApprovedAt.ShouldBe(approvedAt);
        claim.DenialReason.ShouldBe(denialReason);
        claim.PaidAt.ShouldBe(paidAt);
    }
}
