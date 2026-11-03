using PolicyDesk.Domain.Validations;
using Shouldly;

namespace PolicyDesk.Domain.Tests.Validations;

public class PhoneValidatorTests
{
    // ---------- Número local brasileiro (sem "+") ----------

    [Theory(DisplayName = nameof(Deve_SerValido_Quando_NumeroLocalTiverDezOuOnzeDigitos))]
    [Trait("Domain", "PhoneValidator - Local")]
    [InlineData("8333334444")]          // fixo: 10 dígitos (limite inferior)
    [InlineData("83986471687")]         // celular: 11 dígitos (limite superior)
    [InlineData("(83) 3333-4444")]      // máscara é ignorada na contagem
    [InlineData("(83) 9 8647-1687")]
    [InlineData(" 83 98647 1687 ")]
    [InlineData("(55) 9 9999-9999")]    // DDD 55 (RS): sem "+" é número local, não código do país
    public void Deve_SerValido_Quando_NumeroLocalTiverDezOuOnzeDigitos(string phone)
    {
        PhoneValidator.IsValid(phone).ShouldBeTrue();
    }

    [Theory(DisplayName = nameof(Deve_SerInvalido_Quando_NumeroLocalTiverQuantidadeDeDigitosForaDoLimite))]
    [Trait("Domain", "PhoneValidator - Local")]
    [InlineData("833333444")]           // 9 dígitos: um abaixo do mínimo
    [InlineData("839864716870")]        // 12 dígitos: um acima do máximo
    [InlineData("(83) 9 8647-16871")]   // 12 dígitos com máscara
    [InlineData("123")]
    [InlineData("5583986471687")]       // código do país sem "+": ambíguo, por isso o "+" é exigido
    public void Deve_SerInvalido_Quando_NumeroLocalTiverQuantidadeDeDigitosForaDoLimite(string phone)
    {
        PhoneValidator.IsValid(phone).ShouldBeFalse();
    }

    // ---------- Brasil com código do país (+55) ----------

    [Theory(DisplayName = nameof(Deve_SerValido_Quando_TelefoneTiverCodigoDoBrasil))]
    [Trait("Domain", "PhoneValidator - Brasil")]
    [InlineData("+55 (83) 98647-1687")]  // celular: 55 + 11 dígitos
    [InlineData("+55 83 3333-4444")]     // fixo: 55 + 10 dígitos
    [InlineData("+5583986471687")]
    [InlineData(" +55 83 98647-1687")]   // espaço antes do "+"
    [InlineData("+55 (55) 9 9999-9999")] // código do país seguido do DDD 55
    public void Deve_SerValido_Quando_TelefoneTiverCodigoDoBrasil(string phone)
    {
        PhoneValidator.IsValid(phone).ShouldBeTrue();
    }

    [Theory(DisplayName = nameof(Deve_SerInvalido_Quando_NumeroBrasileiroComCodigoTiverTamanhoErrado))]
    [Trait("Domain", "PhoneValidator - Brasil")]
    [InlineData("+55 83 98647-16871")]   // 55 + 12 dígitos: um acima do máximo
    [InlineData("+55 83 3333-444")]      // 55 + 9 dígitos: um abaixo do mínimo
    [InlineData("+55")]
    public void Deve_SerInvalido_Quando_NumeroBrasileiroComCodigoTiverTamanhoErrado(string phone)
    {
        // Para o Brasil a regra é estrita, mesmo cabendo nos limites gerais do E.164
        PhoneValidator.IsValid(phone).ShouldBeFalse();
    }

    // ---------- Outros países (+código, padrão E.164) ----------

    [Theory(DisplayName = nameof(Deve_SerValido_Quando_TelefoneInternacionalEstiverNoPadraoE164))]
    [Trait("Domain", "PhoneValidator - Internacional")]
    [InlineData("+1 (212) 555-0123")]     // Estados Unidos
    [InlineData("+44 20 7946 0958")]      // Reino Unido
    [InlineData("+351 912 345 678")]      // Portugal
    [InlineData("+54 9 11 1234-5678")]    // Argentina
    [InlineData("+86 138 0013 8000")]     // China
    [InlineData("+683 4002")]             // Niue: 7 dígitos (limite inferior do E.164)
    [InlineData("+123 456 789 012 345")]  // 15 dígitos (limite superior do E.164)
    public void Deve_SerValido_Quando_TelefoneInternacionalEstiverNoPadraoE164(string phone)
    {
        PhoneValidator.IsValid(phone).ShouldBeTrue();
    }

    [Theory(DisplayName = nameof(Deve_SerInvalido_Quando_TelefoneInternacionalEstiverForaDoPadraoE164))]
    [Trait("Domain", "PhoneValidator - Internacional")]
    [InlineData("+683 400")]              // 6 dígitos: um abaixo do mínimo
    [InlineData("+123 456 789 012 3456")] // 16 dígitos: um acima do máximo
    [InlineData("+0 21 2555 0123")]       // nenhum código de país começa com 0
    [InlineData("+")]
    [InlineData("+ (--) ----")]
    public void Deve_SerInvalido_Quando_TelefoneInternacionalEstiverForaDoPadraoE164(string phone)
    {
        PhoneValidator.IsValid(phone).ShouldBeFalse();
    }

    // ---------- Entradas sem conteúdo ----------

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

    [Fact(DisplayName = nameof(Deve_ContarSomenteDigitosAscii_Quando_TelefoneTiverDigitosDeOutrosAlfabetos))]
    [Trait("Domain", "PhoneValidator - Local")]
    public void Deve_ContarSomenteDigitosAscii_Quando_TelefoneTiverDigitosDeOutrosAlfabetos()
    {
        // "٨٣٩٨٦٤٧١٦٨٧" são 11 dígitos arábico-índicos: char.IsDigit aceitaria, char.IsAsciiDigit não
        PhoneValidator.IsValid("٨٣٩٨٦٤٧١٦٨٧").ShouldBeFalse();
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
}
