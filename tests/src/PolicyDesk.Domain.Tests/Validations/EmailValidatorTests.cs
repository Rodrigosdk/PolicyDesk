using PolicyDesk.Domain.Validations;
using Shouldly;

namespace PolicyDesk.Domain.Tests.Validations;

public class EmailValidatorTests
{
    [Theory(DisplayName = nameof(Deve_NormalizarEmail_Quando_TiverEspacosOuMaiusculas))]
    [Trait("Domain", "EmailValidator - Normalize")]
    [InlineData("  John.Doe@Example.com  ", "john.doe@example.com")]
    [InlineData("john@example.com", "john@example.com")]
    [InlineData(null, "")]
    [InlineData("   ", "")]
    public void Deve_NormalizarEmail_Quando_TiverEspacosOuMaiusculas(string? input, string expected)
    {
        EmailValidator.Normalize(input).ShouldBe(expected);
    }

    [Theory(DisplayName = nameof(Deve_SerValido_Quando_EmailForCorreto))]
    [Trait("Domain", "EmailValidator - IsValid")]
    [InlineData("john.doe@example.com")]
    [InlineData("john+tag@example.com.br")]
    [InlineData("  John@Example.com ")]
    public void Deve_SerValido_Quando_EmailForCorreto(string email)
    {
        EmailValidator.IsValid(email).ShouldBeTrue();
    }

    [Theory(DisplayName = nameof(Deve_SerInvalido_Quando_EmailForIncorreto))]
    [Trait("Domain", "EmailValidator - IsValid")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("john")]
    [InlineData("john@")]
    [InlineData("@example.com")]
    [InlineData("john@example")]
    [InlineData("john@@example.com")]
    [InlineData("john doe@example.com")]
    [InlineData("John Doe <john@example.com>")]
    public void Deve_SerInvalido_Quando_EmailForIncorreto(string? email)
    {
        EmailValidator.IsValid(email).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_RetornarEmailNormalizado_Quando_EmailForValido))]
    [Trait("Domain", "EmailValidator - NormalizeOrThrow")]
    public void Deve_RetornarEmailNormalizado_Quando_EmailForValido()
    {
        EmailValidator.NormalizeOrThrow(" John@Example.com ").ShouldBe("john@example.com");
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_EmailForInvalido))]
    [Trait("Domain", "EmailValidator - NormalizeOrThrow")]
    [InlineData("invalido")]
    [InlineData("")]
    [InlineData(null)]
    public void Deve_LancarExcecao_Quando_EmailForInvalido(string? email)
    {
        Should.Throw<ArgumentException>(() => EmailValidator.NormalizeOrThrow(email));
    }
}
