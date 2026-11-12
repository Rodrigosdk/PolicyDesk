using PolicyDesk.Domain.Validations;
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
}
