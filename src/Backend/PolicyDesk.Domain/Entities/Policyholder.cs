using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Domain.Entities;

public class Policyholder : Entity
{
    public Policyholder(string name, string cpf, DateOnly birthDate, string email, string phone, DateOnly today)
    {
        Name = NormalizeNameOrThrow(name);
        Cpf = CpfValidator.NormalizeOrThrow(cpf);
        BirthDate = EnsureValidBirthDate(birthDate, today);
        Email = EmailValidator.NormalizeOrThrow(email);
        Phone = EnsureValidPhone(phone);
    }

    // Usado pelo EF Core ao carregar do banco: não reaplica as regras de criação,
    // para que registros gravados antes de uma regra nova continuem legíveis
    private Policyholder()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public bool Active { get; private set; } = true;

    public void Update(string name, DateOnly birthDate, string email, string phone, DateOnly today)
    {
        // Valida tudo antes de alterar qualquer campo
        var normalizedName = NormalizeNameOrThrow(name);
        var validBirthDate = EnsureValidBirthDate(birthDate, today);
        var normalizedEmail = EmailValidator.NormalizeOrThrow(email);
        var validPhone = EnsureValidPhone(phone);

        Name = normalizedName;
        BirthDate = validBirthDate;
        Email = normalizedEmail;
        Phone = validPhone;
        UpdatedAt = DateTime.UtcNow;
    }

    // Chamado ao cadastrar uma apólice. Alterar o segurado é intencional: a gravação muda a versão
    // do registro (xmin), e uma inativação concorrente passa a ser recusada como conflito.
    public void RegisterPolicy()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Active = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeNameOrThrow(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainRuleException(ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED);

        return FieldLimits.Exceeds(name, FieldLimits.PolicyholderName)
            ? throw new DomainRuleException(FieldLimits.PolicyholderNameTooLong)
            : name.Trim();
    }

    private static DateOnly EnsureValidBirthDate(DateOnly birthDate, DateOnly today)
    {
        if (birthDate == default)
            throw new DomainRuleException(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_REQUIRED);

        return birthDate > today
            ? throw new DomainRuleException(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_IN_FUTURE)
            : birthDate;
    }

    private static string EnsureValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainRuleException(ResourcesMessageException.POLICYHOLDER_PHONE_REQUIRED);

        return PhoneValidator.NormalizeOrThrow(phone);
    }
}
