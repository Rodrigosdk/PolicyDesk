using CommonTestUtilities.Entities;
using CommonTestUtilities.Time;
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
    private const string NewPhone = "(83) 9 1234-5678";
    private const string NewPhoneE164 = "+5583912345678";
    private static readonly DateOnly ValidBirthDate = new(1990, 1, 1);
    private static readonly DateOnly NewBirthDate = new(1991, 2, 2);
    private static readonly DateOnly Today = BusinessClockBuilder.DefaultToday;

    private static Policyholder CreatePolicyholder(
        string? name = null,
        string? cpf = null,
        DateOnly? birthDate = null,
        string? email = null,
        string? phone = null) =>
        PolicyholderBuilder.Build(name, cpf, birthDate, email, phone);

    private static Policyholder Create(
        string? name = ValidName,
        string? cpf = ValidCpf,
        DateOnly? birthDate = null,
        string? email = ValidEmail,
        string? phone = ValidPhone) =>
        Policyholder.Create(name!, cpf!, birthDate ?? ValidBirthDate, email!, phone!, Today);

    [Fact(DisplayName = nameof(Deve_Instanciar_Quando_DadosForemValidos))]
    [Trait("Domain", "Policyholders - Instancia")]
    public void Deve_Instanciar_Quando_DadosForemValidos()
    {
        var beforeCreation = DateTime.UtcNow;
        var policyholder = Create();
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
        Create(name: name).Name.ShouldBe(expected);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_NomeForVazio))]
    [Trait("Domain", "Policyholders - Nome")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_LancarExcecao_Quando_NomeForVazio(string? name)
    {
        var exception = Should.Throw<DomainRuleException>(() => Create(name: name));

        exception.Message.ShouldBe(ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED);
    }

    [Theory(DisplayName = nameof(Deve_ArmazenarSomenteDigitos_Quando_CpfTiverMascara))]
    [Trait("Domain", "Policyholders - Cpf")]
    [InlineData("123.456.789-09")]
    [InlineData("12345678909")]
    [InlineData(" 123 456 789 09 ")]
    public void Deve_ArmazenarSomenteDigitos_Quando_CpfTiverMascara(string cpf)
    {
        Create(cpf: cpf).Cpf.ShouldBe(ValidCpfDigits);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_CpfForInvalido))]
    [Trait("Domain", "Policyholders - Cpf")]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("123.456.789-00")]
    [InlineData("111.111.111-11")]
    public void Deve_LancarExcecao_Quando_CpfForInvalido(string cpf)
    {
        var exception = Should.Throw<DomainRuleException>(() => Create(cpf: cpf));

        exception.Message.ShouldBe(ResourcesMessageException.INVALID_CPF);
    }

    [Fact(DisplayName = nameof(Deve_NormalizarEmail_Quando_Criar))]
    [Trait("Domain", "Policyholders - Email")]
    public void Deve_NormalizarEmail_Quando_Criar()
    {
        Create(email: "  John.Doe@Example.com ").Email.ShouldBe(ValidEmail);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_CriarComEmailInvalido))]
    [Trait("Domain", "Policyholders - Email")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("john")]
    [InlineData("john@example")]
    [InlineData("john@@example.com")]
    public void Deve_LancarExcecao_Quando_CriarComEmailInvalido(string email)
    {
        var exception = Should.Throw<DomainRuleException>(() => Create(email: email));

        exception.Message.ShouldBe(ResourcesMessageException.INVALID_EMAIL);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_CriarSemDataDeNascimento))]
    [Trait("Domain", "Policyholders - Nascimento")]
    public void Deve_LancarExcecao_Quando_CriarSemDataDeNascimento()
    {
        var exception = Should.Throw<DomainRuleException>(
            () => Policyholder.Create(ValidName, ValidCpf, default, ValidEmail, ValidPhone, Today));

        exception.Message.ShouldBe(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_REQUIRED);
    }

    [Fact(DisplayName = nameof(Deve_LancarExcecao_Quando_CriarComDataDeNascimentoFutura))]
    [Trait("Domain", "Policyholders - Nascimento")]
    public void Deve_LancarExcecao_Quando_CriarComDataDeNascimentoFutura()
    {
        var exception = Should.Throw<DomainRuleException>(() => Create(birthDate: Today.AddDays(1)));

        exception.Message.ShouldBe(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_IN_FUTURE);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_CriarSemTelefone))]
    [Trait("Domain", "Policyholders - Telefone")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_LancarExcecao_Quando_CriarSemTelefone(string? phone)
    {
        var exception = Should.Throw<DomainRuleException>(() => Create(phone: phone));

        exception.Message.ShouldBe(ResourcesMessageException.POLICYHOLDER_PHONE_REQUIRED);
    }

    [Theory(DisplayName = nameof(Deve_LancarExcecao_Quando_CriarComTelefoneInvalido))]
    [Trait("Domain", "Policyholders - Telefone")]
    [InlineData("123")]
    [InlineData("(83) 8647-168")]
    [InlineData("(83) 9 8647-16871")]
    public void Deve_LancarExcecao_Quando_CriarComTelefoneInvalido(string phone)
    {
        var exception = Should.Throw<DomainRuleException>(() => Create(phone: phone));

        exception.Message.ShouldBe(ResourcesMessageException.INVALID_PHONE);
    }

    [Fact(DisplayName = nameof(Deve_AtualizarOsDados_Quando_DadosForemValidos))]
    [Trait("Domain", "Policyholders - Update")]
    public void Deve_AtualizarOsDados_Quando_DadosForemValidos()
    {
        var policyholder = CreatePolicyholder();

        var beforeUpdate = DateTime.UtcNow;
        policyholder.Update("Jane Doe", NewBirthDate, "jane.doe@example.com", NewPhone, Today);
        var afterUpdate = DateTime.UtcNow;

        policyholder.Name.ShouldBe("Jane Doe");
        policyholder.BirthDate.ShouldBe(NewBirthDate);
        policyholder.Email.ShouldBe("jane.doe@example.com");
        policyholder.Phone.ShouldBe(NewPhoneE164);
        policyholder.UpdatedAt.ShouldNotBeNull().ShouldBeInRange(beforeUpdate, afterUpdate);
    }

    [Fact(DisplayName = nameof(Deve_ManterCpf_Quando_Atualizar))]
    [Trait("Domain", "Policyholders - Update")]
    public void Deve_ManterCpf_Quando_Atualizar()
    {
        var policyholder = CreatePolicyholder();
        var originalCpf = policyholder.Cpf;

        policyholder.Update("Jane Doe", NewBirthDate, "jane.doe@example.com", NewPhone, Today);

        policyholder.Cpf.ShouldBe(originalCpf);
    }

    [Fact(DisplayName = nameof(Deve_NormalizarEmail_Quando_Atualizar))]
    [Trait("Domain", "Policyholders - Update")]
    public void Deve_NormalizarEmail_Quando_Atualizar()
    {
        var policyholder = CreatePolicyholder();

        policyholder.Update("Jane Doe", NewBirthDate, " Jane.Doe@Example.com ", NewPhone, Today);

        policyholder.Email.ShouldBe("jane.doe@example.com");
    }

    [Theory(DisplayName = nameof(Deve_ManterOsDados_Quando_AtualizarComDadosInvalidos))]
    [Trait("Domain", "Policyholders - Update")]
    [InlineData("", "jane.doe@example.com", NewPhone, false)]
    [InlineData("Jane Doe", "invalido", NewPhone, false)]
    [InlineData("Jane Doe", "jane.doe@example.com", "123", false)]
    [InlineData("Jane Doe", "jane.doe@example.com", NewPhone, true)]
    public void Deve_ManterOsDados_Quando_AtualizarComDadosInvalidos(string name, string email, string phone, bool futureBirthDate)
    {
        var policyholder = Create();
        var birthDate = futureBirthDate ? Today.AddDays(1) : NewBirthDate;

        Should.Throw<DomainRuleException>(() => policyholder.Update(name, birthDate, email, phone, Today));

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

        policyholder.UpdatedAt.ShouldNotBeNull().ShouldBeInRange(before, DateTime.UtcNow);
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
