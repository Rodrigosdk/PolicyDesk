using PolicyDesk.Domain.Validations;
using CommonTestUtilities.Requests;
using Shouldly;

namespace PolicyDesk.Domain.Tests.Validations;

public class CpfValidatorTests
{
    [Theory(DisplayName = nameof(Deve_RemoverMascara_Quando_CpfTiverFormatacao))]
    [Trait("Domain", "CpfValidator - Normalize")]
    [InlineData("123.456.789-09", "12345678909")]
    [InlineData(" 123 456 789 09 ", "12345678909")]
    [InlineData("12345678909", "12345678909")]
    public void Deve_RemoverMascara_Quando_CpfTiverFormatacao(string input, string expected)
    {
        CpfValidator.Normalize(input).ShouldBe(expected);
    }

    [Theory(DisplayName = nameof(Deve_RetornarVazio_Quando_CpfForNuloOuEmBranco))]
    [Trait("Domain", "CpfValidator - Normalize")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_RetornarVazio_Quando_CpfForNuloOuEmBranco(string? input)
    {
        CpfValidator.Normalize(input).ShouldBeEmpty();
    }

    [Theory(DisplayName = nameof(Deve_SerValido_Quando_CpfForCorreto))]
    [Trait("Domain", "CpfValidator - IsValid")]
    [InlineData("12345678909")]
    [InlineData("123.456.789-09")]
    [InlineData("52998224725")]
    public void Deve_SerValido_Quando_CpfForCorreto(string cpf)
    {
        CpfValidator.IsValid(cpf).ShouldBeTrue();
    }

    [Fact(DisplayName = nameof(Deve_SerValido_Quando_CpfForGerado))]
    [Trait("Domain", "CpfValidator - IsValid")]
    public void Deve_SerValido_Quando_CpfForGerado()
    {
        CpfGenerator.Generate().ShouldSatisfyAllConditions(
            cpf => CpfValidator.IsValid(cpf).ShouldBeTrue());
    }

    [Theory(DisplayName = nameof(Deve_SerInvalido_Quando_CpfForIncorreto))]
    [Trait("Domain", "CpfValidator - IsValid")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("123456789091")]
    [InlineData("12345678900")]
    [InlineData("11111111111")]
    [InlineData("abcdefghijk")]
    public void Deve_SerInvalido_Quando_CpfForIncorreto(string? cpf)
    {
        CpfValidator.IsValid(cpf).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_RetornarSomenteDigitos_Quando_CpfForValido))]
    [Trait("Domain", "CpfValidator - NormalizeOrThrow")]
    public void Deve_RetornarSomenteDigitos_Quando_CpfForValido()
    {
        CpfValidator.NormalizeOrThrow("123.456.789-09").ShouldBe("12345678909");
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_CpfForInvalido))]
    [Trait("Domain", "CpfValidator - NormalizeOrThrow")]
    [InlineData("123.456.789-00")]
    [InlineData("111.111.111-11")]
    [InlineData("")]
    [InlineData(null)]
    public void Deve_LancarExcecao_Quando_CpfForInvalido(string? cpf)
    {
        Should.Throw<ArgumentException>(() => CpfValidator.NormalizeOrThrow(cpf));
    }

    [Fact(DisplayName = nameof(Deve_AplicarMascara_Quando_CpfTiver11Digitos))]
    [Trait("Domain", "CpfValidator - Format")]
    public void Deve_AplicarMascara_Quando_CpfTiver11Digitos()
    {
        CpfValidator.Format("12345678909").ShouldBe("123.456.789-09");
    }

    [Fact(DisplayName = nameof(Deve_RetornarEntradaOriginal_Quando_CpfNaoTiver11Digitos))]
    [Trait("Domain", "CpfValidator - Format")]
    public void Deve_RetornarEntradaOriginal_Quando_CpfNaoTiver11Digitos()
    {
        CpfValidator.Format("123").ShouldBe("123");
    }
}
