using CommonTestUtilities.Entities;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
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
        var startDate = new DateTime(2026, 1, 2, 12, 30, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2027, 1, 2, 18, 0, 0, DateTimeKind.Utc);
        var beforeCreation = DateTime.UtcNow;

        var policy = PolicyBuilder.Build(
            description: "  Home coverage  ",
            startDate: startDate,
            endDate: endDate);

        var afterCreation = DateTime.UtcNow;
        policy.Id.ShouldNotBe(Guid.Empty);
        policy.Description.ShouldBe("Home coverage");
        policy.StartDate.ShouldBe(startDate.Date);
        policy.EndDate.ShouldBe(endDate.Date);
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
        var today = DateTime.UtcNow.Date;
        var policy = PolicyBuilder.Build(startDate: today.AddDays(-1), endDate: today.AddDays(1));

        policy.IsInForce(today.AddDays(dayOffset).AddHours(12)).ShouldBeTrue();
    }

    [Fact(DisplayName = nameof(Deve_ConsiderarForaDeVigencia_Quando_DataEstiverAntesDoInicio))]
    [Trait("Domain", "Policies - Vigência")]
    public void Deve_ConsiderarForaDeVigencia_Quando_DataEstiverAntesDoInicio()
    {
        var today = DateTime.UtcNow.Date;
        var policy = PolicyBuilder.Build(startDate: today, endDate: today.AddDays(1));

        policy.IsInForce(today.AddDays(-1)).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_ConsiderarForaDeVigencia_Quando_DataEstiverDepoisDoFim))]
    [Trait("Domain", "Policies - Vigência")]
    public void Deve_ConsiderarForaDeVigencia_Quando_DataEstiverDepoisDoFim()
    {
        var today = DateTime.UtcNow.Date;
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
        policy.IsInForce(DateTime.UtcNow).ShouldBeFalse();
        policy.CancelledAt.ShouldNotBeNull();
        policy.CancelledAt.Value.ShouldBeInRange(beforeCancellation, afterCancellation);
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

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_InstanciarComDadosInvalidos))]
    [Trait("Domain", "Policies - Instancia")]
    [InlineData("policyholder", "POLICYHOLDER_REQUIRED")]
    [InlineData("description", "DESCRIPTION_REQUIRED")]
    [InlineData("coverage", "COVERAGE_AMOUNT_MUST_BE_POSITIVE")]
    [InlineData("premium", "PREMIUM_AMOUNT_MUST_BE_POSITIVE")]
    [InlineData("dates", "POLICY_DATES_REQUIRED")]
    [InlineData("endBeforeStart", "POLICY_END_DATE_MUST_BE_AFTER_START_DATE")]
    public void Deve_LancarExcecao_Quando_InstanciarComDadosInvalidos(string invalidField, string expectedMessageKey)
    {
        // InlineData só aceita constantes: o campo inválido chega como texto e o valor é montado aqui
        var today = DateTime.UtcNow.Date;
        var policyholderId = invalidField == "policyholder" ? Guid.Empty : Guid.NewGuid();
        var description = invalidField == "description" ? "   " : "Home coverage";
        var coverage = invalidField == "coverage" ? 0m : 10000m;
        var premium = invalidField == "premium" ? -1m : 100m;
        var startDate = invalidField == "dates" ? default : today;
        var endDate = invalidField == "endBeforeStart" ? today : today.AddDays(30);

        var exception = Should.Throw<DomainRuleException>(
            () => new Policy(policyholderId, description, coverage, premium, startDate, endDate));

        exception.Message.ShouldBe(ResourcesMessageException.ResourceManager.GetString(expectedMessageKey));
    }
}
