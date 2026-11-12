using CommonTestUtilities.Generators;
using CommonTestUtilities.Requests;
using CommonTestUtilities.Security;
using PolicyDesk.Domain.Security.PasswordHashing;
using Shouldly;

namespace PolicyDesk.Infrastructure.Tests.Security.PasswordHashing;

public class Argon2PasswordHasherTests
{
    [Fact(DisplayName = nameof(Deve_GerarHashEmBase64ComSaltEHash_Quando_HashearSenha))]
    [Trait("Infrastructure", "Argon2PasswordHasher - HashPassword")]
    public void Deve_GerarHashEmBase64ComSaltEHash_Quando_HashearSenha()
    {
        var (_, password, passwordHash) = CreateHasherWithHashedPassword();

        passwordHash.ShouldNotBe(password);
        Convert.FromBase64String(passwordHash).Length.ShouldBe(52);
    }

    [Fact(DisplayName = nameof(Deve_GerarHashesDiferentes_Quando_HashearMesmaSenhaDuasVezes))]
    [Trait("Infrastructure", "Argon2PasswordHasher - HashPassword")]
    public void Deve_GerarHashesDiferentes_Quando_HashearMesmaSenhaDuasVezes()
    {
        var (hasher, password, passwordHash) = CreateHasherWithHashedPassword();

        var secondHash = hasher.HashPassword(password);

        secondHash.ShouldNotBe(passwordHash);
    }

    [Fact(DisplayName = nameof(Deve_RetornarVerdadeiro_Quando_SenhaForCorreta))]
    [Trait("Infrastructure", "Argon2PasswordHasher - VerifyPassword")]
    public void Deve_RetornarVerdadeiro_Quando_SenhaForCorreta()
    {
        var (hasher, password, passwordHash) = CreateHasherWithHashedPassword();

        var result = hasher.VerifyPassword(password, passwordHash);

        result.ShouldBeTrue();
    }

    [Fact(DisplayName = nameof(Deve_RetornarFalso_Quando_SenhaForDiferente))]
    [Trait("Infrastructure", "Argon2PasswordHasher - VerifyPassword")]
    public void Deve_RetornarFalso_Quando_SenhaForDiferente()
    {
        var (hasher, _, passwordHash) = CreateHasherWithHashedPassword();
        var wrongPassword = PasswordGenerator.Generate();

        var result = hasher.VerifyPassword(wrongPassword, passwordHash);

        result.ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_RetornarFalso_Quando_SenhaDiferirApenasEmMaiusculas))]
    [Trait("Infrastructure", "Argon2PasswordHasher - VerifyPassword")]
    public void Deve_RetornarFalso_Quando_SenhaDiferirApenasEmMaiusculas()
    {
        var (hasher, password, passwordHash) = CreateHasherWithHashedPassword();

        var result = hasher.VerifyPassword(password.ToUpperInvariant(), passwordHash);

        result.ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_RetornarFalso_Quando_SenhaTiverEspacoAMais))]
    [Trait("Infrastructure", "Argon2PasswordHasher - VerifyPassword")]
    public void Deve_RetornarFalso_Quando_SenhaTiverEspacoAMais()
    {
        var (hasher, password, passwordHash) = CreateHasherWithHashedPassword();

        var result = hasher.VerifyPassword($"{password} ", passwordHash);

        result.ShouldBeFalse();
    }

    private static (IPasswordHashing Hasher, string Password, string PasswordHash) CreateHasherWithHashedPassword()
    {
        var hasher = PasswordHashingBuilder.Build();
        var password = PasswordGenerator.Generate();
        var passwordHash = hasher.HashPassword(password);
        return (hasher, password, passwordHash);
    }
}
