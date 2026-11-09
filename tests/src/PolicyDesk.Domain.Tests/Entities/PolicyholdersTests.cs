using CommonTestUtilities.Entities;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
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
    private const string ValidPhoneE164 = "+5583986471687";
    private static readonly DateTime ValidBirthDate = new(1990, 1, 1);

    private static Policyholder CreatePolicyholder(
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
        policyholder.Phone.ShouldBe(ValidPhoneE164);
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

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_NomeForVazio))]
    [Trait("Domain", "Policyholders - Nome")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_LancarExcecao_Quando_NomeForVazio(string? name)
    {
        Should.Throw<DomainRuleException>(() => new Policyholder(name!, ValidCpf, ValidBirthDate, ValidEmail, ValidPhone));
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
        Should.Throw<DomainRuleException>(() => CreatePolicyholder(cpf: cpf));
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
        Should.Throw<DomainRuleException>(() => CreatePolicyholder(email: email));
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
        policyholder.Phone.ShouldBe("+5583912345678");
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

        Should.Throw<DomainRuleException>(() =>
            policyholder.Update("Jane Doe", new DateTime(1991, 2, 2), "invalido", "(83) 9 1234-5678"));

        policyholder.Name.ShouldBe(ValidName);
        policyholder.BirthDate.ShouldBe(ValidBirthDate);
        policyholder.Email.ShouldBe(ValidEmail);
        policyholder.Phone.ShouldBe(ValidPhoneE164);
        policyholder.UpdatedAt.ShouldBeNull();
    }

    [Theory(DisplayName = nameof(Deve_ManterOsDados_Quando_AtualizarComNomeVazio))]
    [Trait("Domain", "Policyholders - Update")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_ManterOsDados_Quando_AtualizarComNomeVazio(string? name)
    {
        var policyholder = CreatePolicyholder(ValidName, birthDate: ValidBirthDate, email: ValidEmail, phone: ValidPhone);

        Should.Throw<DomainRuleException>(() =>
            policyholder.Update(name!, new DateTime(1991, 2, 2), "jane.doe@example.com", "(83) 9 1234-5678"));

        policyholder.Name.ShouldBe(ValidName);
        policyholder.BirthDate.ShouldBe(ValidBirthDate);
        policyholder.Email.ShouldBe(ValidEmail);
        policyholder.Phone.ShouldBe(ValidPhoneE164);
        policyholder.UpdatedAt.ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_InstanciarSemDataDeNascimento))]
    [Trait("Domain", "Policyholders - Nascimento")]
    public void Deve_LancarExcecao_Quando_InstanciarSemDataDeNascimento()
    {
        var exception = Should.Throw<DomainRuleException>(
            () => new Policyholder(ValidName, ValidCpf, default, ValidEmail, ValidPhone));

        exception.Message.ShouldBe(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_InstanciarComDataDeNascimentoFutura))]
    [Trait("Domain", "Policyholders - Nascimento")]
    public void Deve_LancarExcecao_Quando_InstanciarComDataDeNascimentoFutura()
    {
        var exception = Should.Throw<DomainRuleException>(
            () => new Policyholder(ValidName, ValidCpf, DateTime.UtcNow.Date.AddDays(1), ValidEmail, ValidPhone));

        exception.Message.ShouldBe(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_IN_FUTURE);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_InstanciarSemTelefone))]
    [Trait("Domain", "Policyholders - Telefone")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_LancarExcecao_Quando_InstanciarSemTelefone(string? phone)
    {
        var exception = Should.Throw<DomainRuleException>(
            () => new Policyholder(ValidName, ValidCpf, ValidBirthDate, ValidEmail, phone!));

        exception.Message.ShouldBe(ResourcesMessageException.POLICYHOLDER_PHONE_REQUIRED);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_InstanciarComTelefoneInvalido))]
    [Trait("Domain", "Policyholders - Telefone")]
    [InlineData("123")]
    [InlineData("(83) 8647-168")]
    [InlineData("(83) 9 8647-16871")]
    public void Deve_LancarExcecao_Quando_InstanciarComTelefoneInvalido(string phone)
    {
        var exception = Should.Throw<DomainRuleException>(
            () => new Policyholder(ValidName, ValidCpf, ValidBirthDate, ValidEmail, phone));

        exception.Message.ShouldBe(ResourcesMessageException.INVALID_PHONE);
    }

    [Theory(DisplayName = nameof(Deve_ManterOsDados_Quando_AtualizarComNascimentoOuTelefoneInvalido))]
    [Trait("Domain", "Policyholders - Update")]
    [InlineData(true, "(83) 9 1234-5678")]   // nascimento futuro
    [InlineData(false, "123")]               // telefone inválido
    public void Deve_ManterOsDados_Quando_AtualizarComNascimentoOuTelefoneInvalido(bool futureBirthDate, string phone)
    {
        var policyholder = CreatePolicyholder(ValidName, birthDate: ValidBirthDate, email: ValidEmail, phone: ValidPhone);
        var birthDate = futureBirthDate ? DateTime.UtcNow.Date.AddDays(1) : new DateTime(1991, 2, 2);

        Should.Throw<DomainRuleException>(() =>
            policyholder.Update("Jane Doe", birthDate, "jane.doe@example.com", phone));

        policyholder.Name.ShouldBe(ValidName);
        policyholder.BirthDate.ShouldBe(ValidBirthDate);
        policyholder.Email.ShouldBe(ValidEmail);
        policyholder.Phone.ShouldBe(ValidPhoneE164);
        policyholder.UpdatedAt.ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_RegistrarAlteracao_Quando_ApoliceForCadastrada))]
    [Trait("Domain", "Policyholders - Apólice")]
    public void Deve_RegistrarAlteracao_Quando_ApoliceForCadastrada()
    {
        var policyholder = CreatePolicyholder();
        var before = DateTime.UtcNow;

        policyholder.RegisterPolicy();

        policyholder.UpdatedAt.ShouldNotBeNull();
        policyholder.UpdatedAt.Value.ShouldBeInRange(before, DateTime.UtcNow);
        policyholder.Active.ShouldBeTrue();
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
