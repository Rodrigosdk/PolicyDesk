using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Domain.Tests.Entities;

public class PolicyholdersTests
{
    private const string ValidName = "John Doe";
    private const string ValidCpf = "123.456.789-09";
    private const string ValidCpfDigits = "12345678909";
    private const string ValidEmail = "john.doe@example.com";
    private const string ValidPhone = "(83) 9 8647-1687";
    private static readonly DateTime ValidBirthDate = new(1990, 1, 1);

    private static Policyholders CreatePolicyholder(
        string name = ValidName,
        string cpf = ValidCpf,
        DateTime? birthDate = null,
        string email = ValidEmail,
        string phone = ValidPhone) =>
        new(name, cpf, birthDate ?? ValidBirthDate, email, phone);

    [Fact(DisplayName = nameof(Deve_Instanciar_Quando_DadosForemValidos))]
    [Trait("Domain", "Policyholders - Instancia")]
    public void Deve_Instanciar_Quando_DadosForemValidos()
    {
        var beforeCreation = DateTime.UtcNow;
        var policyholder = CreatePolicyholder();
        var afterCreation = DateTime.UtcNow;

        Assert.NotEqual(Guid.Empty, policyholder.Id);
        Assert.Equal(ValidName, policyholder.Name);
        Assert.Equal(ValidCpfDigits, policyholder.Cpf);
        Assert.Equal(ValidBirthDate, policyholder.BirthDate);
        Assert.Equal(ValidEmail, policyholder.Email);
        Assert.Equal(ValidPhone, policyholder.Phone);
        Assert.InRange(policyholder.CreatedAt, beforeCreation, afterCreation);
        Assert.Null(policyholder.UpdatedAt);
        Assert.False(policyholder.IsDeleted);
    }

    [Theory(DisplayName = nameof(Deve_RemoverEspacosEmBranco_Quando_NomeTiverEspacosNasExtremidades))]
    [Trait("Domain", "Policyholders - Nome")]
    [InlineData("  John Doe  ", "John Doe")]
    [InlineData("  Jane Doe", "Jane Doe")]
    [InlineData("John Doe  ", "John Doe")]
    public void Deve_RemoverEspacosEmBranco_Quando_NomeTiverEspacosNasExtremidades(string name, string expected)
    {
        var policyholder = CreatePolicyholder(name: name);

        Assert.Equal(expected, policyholder.Name);
    }

    [Theory(DisplayName = nameof(Deve_ArmazenarSomenteDigitos_Quando_CpfTiverMascara))]
    [Trait("Domain", "Policyholders - Cpf")]
    [InlineData("123.456.789-09")]
    [InlineData("12345678909")]
    [InlineData(" 123 456 789 09 ")]
    public void Deve_ArmazenarSomenteDigitos_Quando_CpfTiverMascara(string cpf)
    {
        var policyholder = CreatePolicyholder(cpf: cpf);

        Assert.Equal(ValidCpfDigits, policyholder.Cpf);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_CpfForInvalido))]
    [Trait("Domain", "Policyholders - Cpf")]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("123.456.789-00")]
    [InlineData("111.111.111-11")]
    public void Deve_LancarExcecao_Quando_CpfForInvalido(string cpf)
    {
        Assert.Throws<ArgumentException>(() => CreatePolicyholder(cpf: cpf));
    }

    [Fact(DisplayName = nameof(Deve_NormalizarEmail_Quando_Instanciar))]
    [Trait("Domain", "Policyholders - Email")]
    public void Deve_NormalizarEmail_Quando_Instanciar()
    {
        var policyholder = CreatePolicyholder(email: "  John.Doe@Example.com ");

        Assert.Equal(ValidEmail, policyholder.Email);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_InstanciarComEmailInvalido))]
    [Trait("Domain", "Policyholders - Email")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("john")]
    [InlineData("john@example")]
    [InlineData("john@@example.com")]
    public void Deve_LancarExcecao_Quando_InstanciarComEmailInvalido(string email)
    {
        Assert.Throws<ArgumentException>(() => CreatePolicyholder(email: email));
    }

    [Fact(DisplayName = nameof(Deve_AtualizarOsDados_Quando_DadosForemValidos))]
    [Trait("Domain", "Policyholders - Update")]
    public void Deve_AtualizarOsDados_Quando_DadosForemValidos()
    {
        var policyholder = CreatePolicyholder();
        var newBirthDate = new DateTime(1991, 2, 2);

        var beforeUpdate = DateTime.UtcNow;
        policyholder.Update("Jane Doe", newBirthDate, "jane.doe@example.com", "(83) 9 1234-5678");
        var afterUpdate = DateTime.UtcNow;

        Assert.Equal("Jane Doe", policyholder.Name);
        Assert.Equal(newBirthDate, policyholder.BirthDate);
        Assert.Equal("jane.doe@example.com", policyholder.Email);
        Assert.Equal("(83) 9 1234-5678", policyholder.Phone);
        Assert.NotNull(policyholder.UpdatedAt);
        Assert.InRange(policyholder.UpdatedAt.Value, beforeUpdate, afterUpdate);
    }

    [Fact(DisplayName = nameof(Deve_ManterCpf_Quando_Atualizar))]
    [Trait("Domain", "Policyholders - Update")]
    public void Deve_ManterCpf_Quando_Atualizar()
    {
        var policyholder = CreatePolicyholder();

        policyholder.Update("Jane Doe", new DateTime(1991, 2, 2), "jane.doe@example.com", "(83) 9 1234-5678");

        Assert.Equal(ValidCpfDigits, policyholder.Cpf);
    }

    [Fact(DisplayName = nameof(Deve_NormalizarEmail_Quando_Atualizar))]
    [Trait("Domain", "Policyholders - Update")]
    public void Deve_NormalizarEmail_Quando_Atualizar()
    {
        var policyholder = CreatePolicyholder();

        policyholder.Update("Jane Doe", new DateTime(1991, 2, 2), " Jane.Doe@Example.com ", "(83) 9 1234-5678");

        Assert.Equal("jane.doe@example.com", policyholder.Email);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecaoEManterEmail_Quando_AtualizarComEmailInvalido))]
    [Trait("Domain", "Policyholders - Update")]
    public void Deve_LancarExcecaoEManterEmail_Quando_AtualizarComEmailInvalido()
    {
        var policyholder = CreatePolicyholder();

        Assert.Throws<ArgumentException>(() =>
            policyholder.Update("Jane Doe", new DateTime(1991, 2, 2), "invalido", "(83) 9 1234-5678"));

        Assert.Equal(ValidEmail, policyholder.Email);
    }

    [Fact(DisplayName = nameof(Deve_Inativar_Quando_Solicitado))]
    [Trait("Domain", "Policyholders - Deactivate")]
    public void Deve_Inativar_Quando_Solicitado()
    {
        var policyholder = CreatePolicyholder();

        policyholder.Deactivate();

        Assert.True(policyholder.IsDeleted);
        Assert.NotNull(policyholder.UpdatedAt);
    }
}
