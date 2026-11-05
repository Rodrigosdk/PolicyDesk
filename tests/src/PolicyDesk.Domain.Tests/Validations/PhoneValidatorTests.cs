using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Domain.Tests.Validations;

public class PhoneValidatorTests
{
    [Theory(DisplayName = nameof(Deve_SerValido_Quando_NumeroLocalBrasileiroForValido))]
    [Trait("Domain", "PhoneValidator - Local")]
    [InlineData("8333334444")]
    [InlineData("83986471687")]
    [InlineData("(83) 3333-4444")]
    [InlineData("(83) 9 8647-1687")]
    [InlineData(" 83 98647 1687 ")]
    [InlineData("(55) 9 9999-9999")]
    public void Deve_SerValido_Quando_NumeroLocalBrasileiroForValido(string phone)
    {
        PhoneValidator.IsValid(phone).ShouldBeTrue();
    }

    [Theory(DisplayName = nameof(Deve_SerInvalido_Quando_NumeroLocalBrasileiroForInvalido))]
    [Trait("Domain", "PhoneValidator - Local")]
    [InlineData("833333444")]
    [InlineData("839864716870")]
    [InlineData("(83) 9 8647-16871")]
    [InlineData("123")]
    [InlineData("(20) 9 8647-1687")]
    [InlineData("(83) 1 8647-1687")]
    public void Deve_SerInvalido_Quando_NumeroLocalBrasileiroForInvalido(string phone)
    {
        PhoneValidator.IsValid(phone).ShouldBeFalse();
    }

    [Theory(DisplayName = nameof(Deve_SerValido_Quando_TelefoneTiverCodigoDoBrasil))]
    [Trait("Domain", "PhoneValidator - Brasil")]
    [InlineData("+55 (83) 98647-1687")]
    [InlineData("+55 83 3333-4444")]
    [InlineData("+5583986471687")]
    [InlineData(" +55 83 98647-1687")]
    [InlineData("+55 (55) 9 9999-9999")]
    public void Deve_SerValido_Quando_TelefoneTiverCodigoDoBrasil(string phone)
    {
        PhoneValidator.IsValid(phone).ShouldBeTrue();
    }

    [Theory(DisplayName = nameof(Deve_SerInvalido_Quando_NumeroBrasileiroComCodigoForInvalido))]
    [Trait("Domain", "PhoneValidator - Brasil")]
    [InlineData("+55 83 98647-16871")]
    [InlineData("+55 83 3333-444")]
    [InlineData("+55")]
    [InlineData("+55 20 98647-1687")]
    public void Deve_SerInvalido_Quando_NumeroBrasileiroComCodigoForInvalido(string phone)
    {
        PhoneValidator.IsValid(phone).ShouldBeFalse();
    }

    [Theory(DisplayName = nameof(Deve_SerValido_Quando_TelefoneInternacionalForValidoNoPais))]
    [Trait("Domain", "PhoneValidator - Internacional")]
    [InlineData("+1 (212) 555-0123")]
    [InlineData("+44 20 7946 0958")]
    [InlineData("+351 912 345 678")]
    [InlineData("+54 9 11 1234-5678")]
    [InlineData("+86 138 0013 8000")]
    public void Deve_SerValido_Quando_TelefoneInternacionalForValidoNoPais(string phone)
    {
        PhoneValidator.IsValid(phone).ShouldBeTrue();
    }

    [Theory(DisplayName = nameof(Deve_SerInvalido_Quando_TelefoneInternacionalForInvalidoNoPais))]
    [Trait("Domain", "PhoneValidator - Internacional")]
    [InlineData("+44 20 7946")]
    [InlineData("+1 212 555 01234 5")]
    [InlineData("+999 1234 5678")]
    [InlineData("+0 21 2555 0123")]
    [InlineData("+")]
    [InlineData("+ (--) ----")]
    public void Deve_SerInvalido_Quando_TelefoneInternacionalForInvalidoNoPais(string phone)
    {
        PhoneValidator.IsValid(phone).ShouldBeFalse();
    }

    [Theory(DisplayName = nameof(Deve_SerInvalido_Quando_TelefoneForNuloVazioOuSemDigitos))]
    [Trait("Domain", "PhoneValidator - Local")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("telefone")]
    [InlineData("(--) ----- ----")]
    public void Deve_SerInvalido_Quando_TelefoneForNuloVazioOuSemDigitos(string? phone)
    {
        PhoneValidator.IsValid(phone).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_SerInvalido_Quando_TelefoneTiverDigitosDeOutrosAlfabetos))]
    [Trait("Domain", "PhoneValidator - Local")]
    public void Deve_SerInvalido_Quando_TelefoneTiverDigitosDeOutrosAlfabetos()
    {
        PhoneValidator.IsValid("٨٣٩٨٦٤٧١٦٨٧").ShouldBeFalse();
    }

    [Theory(DisplayName = nameof(Deve_SerInvalido_Quando_TelefoneTiverRamal))]
    [Trait("Domain", "PhoneValidator - Local")]
    [InlineData("8333334444 ext. 12")]
    [InlineData("+44 20 7946 0958 x1")]
    public void Deve_SerInvalido_Quando_TelefoneTiverRamal(string phone)
    {
        PhoneValidator.IsValid(phone).ShouldBeFalse();
    }

    [Theory(DisplayName = nameof(Deve_SerInvalido_Quando_TelefoneExcederOTamanhoMaximo))]
    [Trait("Domain", "PhoneValidator - Tamanho")]
    [InlineData("(83) 98647-1687 celular")]
    [InlineData("83986471687                    ")]
    public void Deve_SerInvalido_Quando_TelefoneExcederOTamanhoMaximo(string phone)
    {
        phone.Length.ShouldBeGreaterThan(FieldLimits.Phone);
        PhoneValidator.IsValid(phone).ShouldBeFalse();
    }

    [Theory(DisplayName = nameof(Deve_NormalizarParaE164_Quando_TelefoneForValido))]
    [Trait("Domain", "PhoneValidator - Normalização")]
    [InlineData("(83) 9 8647-1687", "+5583986471687")]
    [InlineData("83 3333-4444", "+558333334444")]
    [InlineData("+55 (83) 98647-1687", "+5583986471687")]
    [InlineData(" +55 83 98647-1687 ", "+5583986471687")]
    [InlineData("+44 20 7946 0958", "+442079460958")]
    [InlineData("+1 (212) 555-0123", "+12125550123")]
    public void Deve_NormalizarParaE164_Quando_TelefoneForValido(string phone, string expected)
    {
        PhoneValidator.NormalizeOrThrow(phone).ShouldBe(expected);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_NormalizarTelefoneInvalido))]
    [Trait("Domain", "PhoneValidator - Normalização")]
    [InlineData(null)]
    [InlineData("(20) 9 8647-1687")]
    [InlineData("+999 1234 5678")]
    public void Deve_LancarExcecao_Quando_NormalizarTelefoneInvalido(string? phone)
    {
        var exception = Should.Throw<DomainRuleException>(() => PhoneValidator.NormalizeOrThrow(phone));

        exception.Message.ShouldBe(ResourcesMessageException.INVALID_PHONE);
    }
}
