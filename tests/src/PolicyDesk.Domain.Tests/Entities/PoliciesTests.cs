using CommonTestUtilities.Entities;
using CommonTestUtilities.Time;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Domain.Tests.Entities;

public class PoliciesTests
{
    [Fact(DisplayName = nameof(Deve_InstanciarApoliceAtiva_Quando_DadosForemValidos))]
    [Trait("Domain", "Policies - Instancia")]
    public void Deve_InstanciarApoliceAtiva_Quando_DadosForemValidos()
    {
        var startDate = new DateOnly(2026, 1, 2);
        var endDate = new DateOnly(2027, 1, 2);
        var beforeCreation = DateTime.UtcNow;

        var policyholderId = Guid.NewGuid();

        var policy = Policy.Create(policyholderId, "  Home coverage  ", 10000m, 100m, startDate, endDate);

        var afterCreation = DateTime.UtcNow;
        policy.Id.ShouldNotBe(Guid.Empty);
        policy.PolicyholderId.ShouldBe(policyholderId);
        policy.Description.ShouldBe("Home coverage");
        policy.CoverageAmount.ShouldBe(10000m);
        policy.PremiumAmount.ShouldBe(100m);
        policy.StartDate.ShouldBe(startDate);
        policy.EndDate.ShouldBe(endDate);
        policy.Status.ShouldBe(PolicyStatus.Active);
        policy.IsCancelled.ShouldBeFalse();
        policy.CreatedAt.ShouldBeInRange(beforeCreation, afterCreation);
        policy.UpdatedAt.ShouldBeNull();
        policy.CancelledAt.ShouldBeNull();
    }

    [Theory(DisplayName = nameof(Deve_ConsiderarVigente_Quando_DataEstiverNosLimites))]
    [Trait("Domain", "Policies - Vigência")]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void Deve_ConsiderarVigente_Quando_DataEstiverNosLimites(int dayOffset)
    {
        var today = BusinessClockBuilder.DefaultToday;
        var policy = PolicyBuilder.Build(startDate: today.AddDays(-1), endDate: today.AddDays(1));

        policy.IsInForce(today.AddDays(dayOffset)).ShouldBeTrue();
    }

    [Fact(DisplayName = nameof(Deve_ConsiderarForaDeVigencia_Quando_DataEstiverAntesDoInicio))]
    [Trait("Domain", "Policies - Vigência")]
    public void Deve_ConsiderarForaDeVigencia_Quando_DataEstiverAntesDoInicio()
    {
        var today = BusinessClockBuilder.DefaultToday;
        var policy = PolicyBuilder.Build(startDate: today, endDate: today.AddDays(1));

        policy.IsInForce(today.AddDays(-1)).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_ConsiderarForaDeVigencia_Quando_DataEstiverDepoisDoFim))]
    [Trait("Domain", "Policies - Vigência")]
    public void Deve_ConsiderarForaDeVigencia_Quando_DataEstiverDepoisDoFim()
    {
        var today = BusinessClockBuilder.DefaultToday;
        var policy = PolicyBuilder.Build(startDate: today.AddDays(-1), endDate: today);

        policy.IsInForce(today.AddDays(1)).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_CancelarERegistrarDatas_Quando_Solicitado))]
    [Trait("Domain", "Policies - Cancelamento")]
    public void Deve_CancelarERegistrarDatas_Quando_Solicitado()
    {
        var policy = PolicyBuilder.Build();
        var beforeCancellation = DateTime.UtcNow;

        policy.Cancel();

        var afterCancellation = DateTime.UtcNow;
        policy.Status.ShouldBe(PolicyStatus.Cancelled);
        policy.IsCancelled.ShouldBeTrue();
        policy.IsInForce(BusinessClockBuilder.DefaultToday).ShouldBeFalse();
        policy.CancelledAt.ShouldNotBeNull().ShouldBeInRange(beforeCancellation, afterCancellation);
        policy.UpdatedAt.ShouldBe(policy.CancelledAt);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoSemAlterarDatas_Quando_CancelarApoliceJaCancelada))]
    [Trait("Domain", "Policies - Cancelamento")]
    public void Deve_LancarExcecaoSemAlterarDatas_Quando_CancelarApoliceJaCancelada()
    {
        var policy = PolicyBuilder.Build();
        policy.Cancel();
        var cancelledAt = policy.CancelledAt;

        var exception = Should.Throw<DomainRuleException>(() => policy.Cancel());

        exception.Message.ShouldBe(ResourcesMessageException.POLICY_ALREADY_CANCELLED);
        policy.Status.ShouldBe(PolicyStatus.Cancelled);
        policy.CancelledAt.ShouldBe(cancelledAt);
        policy.UpdatedAt.ShouldBe(cancelledAt);
    }

    [Fact(DisplayName = nameof(Deve_Criar_Quando_DescricaoTiverExatamenteOLimite))]
    [Trait("Domain", "Policies - Instancia")]
    public void Deve_Criar_Quando_DescricaoTiverExatamenteOLimite()
    {
        var today = BusinessClockBuilder.DefaultToday;
        var description = new string('a', FieldLimits.Description);

        var policy = Policy.Create(Guid.NewGuid(), $"  {description}  ", 10000m, 100m, today, today.AddDays(30));

        policy.Description.ShouldBe(description);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_CriarComDescricaoAcimaDoLimite))]
    [Trait("Domain", "Policies - Instancia")]
    public void Deve_LancarExcecao_Quando_CriarComDescricaoAcimaDoLimite()
    {
        var today = BusinessClockBuilder.DefaultToday;
        var description = new string('a', FieldLimits.Description + 1);

        var exception = Should.Throw<DomainRuleException>(
            () => Policy.Create(Guid.NewGuid(), description, 10000m, 100m, today, today.AddDays(30)));

        exception.Message.ShouldBe(FieldLimits.DescriptionTooLong);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_CriarComValoresComMaisDeDuasCasasDecimais))]
    [Trait("Domain", "Policies - Instancia")]
    [InlineData(true)]
    [InlineData(false)]
    public void Deve_LancarExcecao_Quando_CriarComValoresComMaisDeDuasCasasDecimais(bool invalidCoverage)
    {
        var today = BusinessClockBuilder.DefaultToday;
        var coverage = invalidCoverage ? 1000.001m : 1000m;
        var premium = invalidCoverage ? 100m : 100.001m;

        var exception = Should.Throw<DomainRuleException>(
            () => Policy.Create(Guid.NewGuid(), "Home coverage", coverage, premium, today, today.AddDays(30)));

        exception.Message.ShouldBe(invalidCoverage
            ? MoneyLimits.CoverageAmountInvalidPrecision
            : MoneyLimits.PremiumAmountInvalidPrecision);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_CriarComDadosInvalidos))]
    [Trait("Domain", "Policies - Instancia")]
    [InlineData("policyholder", "POLICYHOLDER_REQUIRED")]
    [InlineData("description", "DESCRIPTION_REQUIRED")]
    [InlineData("coverage", "COVERAGE_AMOUNT_MUST_BE_POSITIVE")]
    [InlineData("premium", "PREMIUM_AMOUNT_MUST_BE_POSITIVE")]
    [InlineData("dates", "POLICY_DATES_REQUIRED")]
    [InlineData("endBeforeStart", "POLICY_END_DATE_MUST_BE_AFTER_START_DATE")]
    public void Deve_LancarExcecao_Quando_CriarComDadosInvalidos(string invalidField, string expectedMessageKey)
    {
        var today = BusinessClockBuilder.DefaultToday;
        var policyholderId = invalidField == "policyholder" ? Guid.Empty : Guid.NewGuid();
        var description = invalidField == "description" ? "   " : "Home coverage";
        var coverage = invalidField == "coverage" ? 0m : 10000m;
        var premium = invalidField == "premium" ? -1m : 100m;
        var startDate = invalidField == "dates" ? default : today;
        var endDate = invalidField == "endBeforeStart" ? today : today.AddDays(30);

        var exception = Should.Throw<DomainRuleException>(
            () => Policy.Create(policyholderId, description, coverage, premium, startDate, endDate));

        exception.Message.ShouldBe(ResourcesMessageException.ResourceManager.GetString(expectedMessageKey, ResourcesMessageException.Culture));
    }

    [Theory(DisplayName = nameof(Deve_AceitarSinistroEAtualizarApolice_Quando_ValorCouberNoSaldo))]
    [Trait("Domain", "Policies - Sinistro")]
    [InlineData(0, 500)]
    [InlineData(8000, 2000)]
    [InlineData(0, 10000)]
    public void Deve_AceitarSinistroEAtualizarApolice_Quando_ValorCouberNoSaldo(decimal committedAmount, decimal amount)
    {
        var policy = PolicyBuilder.Build(coverageAmount: 10000);
        var beforeRegistration = DateTime.UtcNow;

        policy.RegisterClaim(amount, committedAmount);

        policy.UpdatedAt.ShouldNotBeNull().ShouldBeGreaterThanOrEqualTo(beforeRegistration);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecaoSemAtualizar_Quando_ValorExcederSaldo))]
    [Trait("Domain", "Policies - Sinistro")]
    [InlineData(0, 10000.01)]
    [InlineData(8000, 8000)]
    [InlineData(10000, 0.01)]
    public void Deve_LancarExcecaoSemAtualizar_Quando_ValorExcederSaldo(decimal committedAmount, decimal amount)
    {
        var policy = PolicyBuilder.Build(coverageAmount: 10000);

        var exception = Should.Throw<DomainRuleException>(() => policy.RegisterClaim(amount, committedAmount));

        exception.Message.ShouldBe(ResourcesMessageException.CLAIM_AMOUNT_EXCEEDS_COVERAGE);
        policy.UpdatedAt.ShouldBeNull();
    }
}
