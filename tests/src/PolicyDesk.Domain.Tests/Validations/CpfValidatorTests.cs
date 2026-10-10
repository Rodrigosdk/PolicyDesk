using PolicyDesk.Domain.Validations;

namespace PolicyDesk.Domain.Tests.Validations;

public class CpfValidatorTests
{
    [Theory(DisplayName = nameof(Deve_RemoverMascara_Quando_CpfTiverFormatacao))]
    [Trait("Domain", "CpfValidator - Normalize")]
    [InlineData("123.456.789-09", "12345678909")]
    [InlineData(" 123 456 789 09 ", "12345678909")]
    [InlineData("12345678909", "12345678909")]
    public void Deve_RemoverMascara_Quando_CpfTiverFormatacao(string entrada, string esperado)
    {
        Assert.Equal(esperado, CpfValidator.Normalize(entrada));
    }

    [Theory(DisplayName = nameof(Deve_RetornarVazio_Quando_CpfForNuloOuEmBranco))]
    [Trait("Domain", "CpfValidator - Normalize")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_RetornarVazio_Quando_CpfForNuloOuEmBranco(string? entrada)
    {
        Assert.Equal(string.Empty, CpfValidator.Normalize(entrada));
    }

    [Theory(DisplayName = nameof(Deve_SerValido_Quando_CpfForCorreto))]
    [Trait("Domain", "CpfValidator - IsValid")]
    [InlineData("12345678909")]
    [InlineData("123.456.789-09")]
    [InlineData("52998224725")]
    public void Deve_SerValido_Quando_CpfForCorreto(string cpf)
    {
        Assert.True(CpfValidator.IsValid(cpf));
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
        Assert.False(CpfValidator.IsValid(cpf));
    }

    [Fact(DisplayName = nameof(Deve_RetornarSomenteDigitos_Quando_CpfForValido))]
    [Trait("Domain", "CpfValidator - NormalizeOrThrow")]
    public void Deve_RetornarSomenteDigitos_Quando_CpfForValido()
    {
        Assert.Equal("12345678909", CpfValidator.NormalizeOrThrow("123.456.789-09"));
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_CpfForInvalido))]
    [Trait("Domain", "CpfValidator - NormalizeOrThrow")]
    [InlineData("123.456.789-00")]
    [InlineData("111.111.111-11")]
    [InlineData("")]
    [InlineData(null)]
    public void Deve_LancarExcecao_Quando_CpfForInvalido(string? cpf)
    {
        Assert.Throws<ArgumentException>(() => CpfValidator.NormalizeOrThrow(cpf));
    }

    [Fact(DisplayName = nameof(Deve_AplicarMascara_Quando_CpfTiver11Digitos))]
    [Trait("Domain", "CpfValidator - Format")]
    public void Deve_AplicarMascara_Quando_CpfTiver11Digitos()
    {
        Assert.Equal("123.456.789-09", CpfValidator.Format("12345678909"));
    }

    [Fact(DisplayName = nameof(Deve_RetornarEntradaOriginal_Quando_CpfNaoTiver11Digitos))]
    [Trait("Domain", "CpfValidator - Format")]
    public void Deve_RetornarEntradaOriginal_Quando_CpfNaoTiver11Digitos()
    {
        Assert.Equal("123", CpfValidator.Format("123"));
    }
}