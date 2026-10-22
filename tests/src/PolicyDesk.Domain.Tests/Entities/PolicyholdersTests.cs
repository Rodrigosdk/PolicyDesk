using CommonTestUtilities.Entities;
using Shouldly;
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
        string? name = null,
        string? cpf = null,
        DateTime? birthDate = null,
        string? email = null,
        string? phone = null) =>
        PolicyholderBuilder.Build(name, cpf, birthDate, email, phone);

    [Fact(DisplayName = nameof(Deve_Instanciar_Quando_DadosForemValidos))]
    [Trait("Domain", "Policyholders - Instancia")]
    public void Deve_Instanciar_Quando_DadosForemValidos()
    {
        var beforeCreation = DateTime.UtcNow;
        var policyholder = CreatePolicyholder(ValidName, ValidCpf, ValidBirthDate, ValidEmail, ValidPhone);
        var afterCreation = DateTime.UtcNow;

        policyholder.Id.ShouldNotBe(Guid.Empty);
        policyholder.Name.ShouldBe(ValidName);
        policyholder.Cpf.ShouldBe(ValidCpfDigits);
        policyholder.BirthDate.ShouldBe(ValidBirthDate);
        policyholder.Email.ShouldBe(ValidEmail);
        policyholder.Phone.ShouldBe(ValidPhone);
        policyholder.CreatedAt.ShouldBeInRange(beforeCreation, afterCreation);
        policyholder.UpdatedAt.ShouldBeNull();
        policyholder.Active.ShouldBeTrue();
    }

    [Theory(DisplayName = nameof(Deve_RemoverEspacosEmBranco_Quando_NomeTiverEspacosNasExtremidades))]
    [Trait("Domain", "Policyholders - Nome")]
    [InlineData("  John Doe  ", "John Doe")]
    [InlineData("  Jane Doe", "Jane Doe")]
    [InlineData("John Doe  ", "John Doe")]
    public void Deve_RemoverEspacosEmBranco_Quando_NomeTiverEspacosNasExtremidades(string name, string expected)
    {
        CreatePolicyholder(name: name).Name.ShouldBe(expected);
    }

    [Theory(DisplayName = nameof(Deve_ArmazenarSomenteDigitos_Quando_CpfTiverMascara))]
    [Trait("Domain", "Policyholders - Cpf")]
    [InlineData("123.456.789-09")]
    [InlineData("12345678909")]
    [InlineData(" 123 456 789 09 ")]
    public void Deve_ArmazenarSomenteDigitos_Quando_CpfTiverMascara(string cpf)
    {
        CreatePolicyholder(cpf: cpf).Cpf.ShouldBe(ValidCpfDigits);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_CpfForInvalido))]
    [Trait("Domain", "Policyholders - Cpf")]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("123.456.789-00")]
    [InlineData("111.111.111-11")]
    public void Deve_LancarExcecao_Quando_CpfForInvalido(string cpf)
    {
        Should.Throw<ArgumentException>(() => CreatePolicyholder(cpf: cpf));
    }

    [Fact(DisplayName = nameof(Deve_NormalizarEmail_Quando_Instanciar))]
    [Trait("Domain", "Policyholders - Email")]
    public void Deve_NormalizarEmail_Quando_Instanciar()
    {
        CreatePolicyholder(email: "  John.Doe@Example.com ").Email.ShouldBe(ValidEmail);
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
        Should.Throw<ArgumentException>(() => CreatePolicyholder(email: email));
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

        policyholder.Name.ShouldBe("Jane Doe");
        policyholder.BirthDate.ShouldBe(newBirthDate);
        policyholder.Email.ShouldBe("jane.doe@example.com");
        policyholder.Phone.ShouldBe("(83) 9 1234-5678");
        policyholder.UpdatedAt.ShouldNotBeNull();
        policyholder.UpdatedAt.Value.ShouldBeInRange(beforeUpdate, afterUpdate);
    }

    [Fact(DisplayName = nameof(Deve_ManterCpf_Quando_Atualizar))]
    [Trait("Domain", "Policyholders - Update")]
    public void Deve_ManterCpf_Quando_Atualizar()
    {
        var policyholder = CreatePolicyholder();
        var originalCpf = policyholder.Cpf;

        policyholder.Update("Jane Doe", new DateTime(1991, 2, 2), "jane.doe@example.com", "(83) 9 1234-5678");

        policyholder.Cpf.ShouldBe(originalCpf);
    }

    [Fact(DisplayName = nameof(Deve_NormalizarEmail_Quando_Atualizar))]
    [Trait("Domain", "Policyholders - Update")]
    public void Deve_NormalizarEmail_Quando_Atualizar()
    {
        var policyholder = CreatePolicyholder();

        policyholder.Update("Jane Doe", new DateTime(1991, 2, 2), " Jane.Doe@Example.com ", "(83) 9 1234-5678");

        policyholder.Email.ShouldBe("jane.doe@example.com");
    }

    [Fact(DisplayName = nameof(Deve_ManterOsDados_Quando_AtualizarComEmailInvalido))]
    [Trait("Domain", "Policyholders - Update")]
    public void Deve_ManterOsDados_Quando_AtualizarComEmailInvalido()
    {
        var policyholder = CreatePolicyholder(ValidName, birthDate: ValidBirthDate, email: ValidEmail, phone: ValidPhone);

        Should.Throw<ArgumentException>(() =>
            policyholder.Update("Jane Doe", new DateTime(1991, 2, 2), "invalido", "(83) 9 1234-5678"));

        policyholder.Name.ShouldBe(ValidName);
        policyholder.BirthDate.ShouldBe(ValidBirthDate);
        policyholder.Email.ShouldBe(ValidEmail);
        policyholder.Phone.ShouldBe(ValidPhone);
        policyholder.UpdatedAt.ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_Inativar_Quando_Solicitado))]
    [Trait("Domain", "Policyholders - Deactivate")]
    public void Deve_Inativar_Quando_Solicitado()
    {
        var policyholder = CreatePolicyholder();

        policyholder.Deactivate();

        policyholder.Active.ShouldBeFalse();
        policyholder.UpdatedAt.ShouldNotBeNull();
    }
}
