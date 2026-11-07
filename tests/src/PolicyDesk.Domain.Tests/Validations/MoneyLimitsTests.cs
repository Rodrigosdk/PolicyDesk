using CommonTestUtilities.Entities;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Domain.Tests.Validations;

public class MoneyLimitsTests
{
    [Theory(DisplayName = nameof(Deve_AceitarValor_Quando_CouberNaPrecisaoDaColuna))]
    [Trait("Domain", "Valores monetários")]
    [InlineData("0.01")]
    [InlineData("100")]
    [InlineData("100.5")]
    [InlineData("100.50")]
    [InlineData("9999999999999999.99")]
    public void Deve_AceitarValor_Quando_CouberNaPrecisaoDaColuna(string value)
    {
        MoneyLimits.IsValid(decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture)).ShouldBeTrue();
    }

    [Theory(DisplayName = nameof(Deve_RecusarValor_Quando_ExcederPrecisaoDaColuna))]
    [Trait("Domain", "Valores monetários")]
    [InlineData("100.001")]
    [InlineData("100.123456789")]
    [InlineData("10000000000000000")]
    public void Deve_RecusarValor_Quando_ExcederPrecisaoDaColuna(string value)
    {
        MoneyLimits.IsValid(decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture)).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_CriarSinistroComMaisDeDuasCasasDecimais))]
    [Trait("Domain", "Valores monetários")]
    public void Deve_LancarExcecao_Quando_CriarSinistroComMaisDeDuasCasasDecimais()
    {
        var exception = Should.Throw<DomainRuleException>(
            () => new Claim(Guid.NewGuid(), "Description", 10.005m, DateTime.UtcNow.Date));

        exception.Message.ShouldBe(MoneyLimits.ClaimAmountInvalidPrecision);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_CriarApoliceComMaisDeDuasCasasDecimais))]
    [Trait("Domain", "Valores monetários")]
    [InlineData(true)]
    [InlineData(false)]
    public void Deve_LancarExcecao_Quando_CriarApoliceComMaisDeDuasCasasDecimais(bool invalidCoverage)
    {
        var exception = Should.Throw<DomainRuleException>(() => PolicyBuilder.Build(
            coverageAmount: invalidCoverage ? 1000.001m : 1000m,
            premiumAmount: invalidCoverage ? 100m : 100.001m));

        exception.Message.ShouldBe(invalidCoverage
            ? MoneyLimits.CoverageAmountInvalidPrecision
            : MoneyLimits.PremiumAmountInvalidPrecision);
    }
}
