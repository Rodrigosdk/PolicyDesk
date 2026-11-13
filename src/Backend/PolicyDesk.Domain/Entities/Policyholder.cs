using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Domain.Entities;

public class Policyholder : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public bool Active { get; private set; } = true;

    private Policyholder()
    {
    }

    public static Policyholder Create(string name, string cpf, DateOnly birthDate, string email, string phone, DateOnly today)
    {
        ValidateCpf(cpf);
        Validate(name, birthDate, email, phone, today);

        return new Policyholder
        {
            Name = name.Trim(),
            Cpf = CpfValidator.Normalize(cpf),
            BirthDate = birthDate,
            Email = EmailValidator.Normalize(email),
            Phone = PhoneValidator.NormalizeOrThrow(phone)
        };
    }

    public void Update(string name, DateOnly birthDate, string email, string phone, DateOnly today)
    {
        Validate(name, birthDate, email, phone, today);

        Name = name.Trim();
        BirthDate = birthDate;
        Email = EmailValidator.Normalize(email);
        Phone = PhoneValidator.NormalizeOrThrow(phone);
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

    private static void ValidateCpf(string cpf)
    {
        if (!CpfValidator.IsValid(cpf))
            throw new DomainRuleException(ResourcesMessageException.INVALID_CPF);
    }

    private static void Validate(string name, DateOnly birthDate, string email, string phone, DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainRuleException(ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED);
        if (FieldLimits.Exceeds(name, FieldLimits.PolicyholderName))
            throw new DomainRuleException(FieldLimits.PolicyholderNameTooLong);
        if (birthDate == default)
            throw new DomainRuleException(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_REQUIRED);
        if (birthDate > today)
            throw new DomainRuleException(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_IN_FUTURE);
        if (!EmailValidator.IsValid(email))
            throw new DomainRuleException(ResourcesMessageException.INVALID_EMAIL);
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainRuleException(ResourcesMessageException.POLICYHOLDER_PHONE_REQUIRED);
        if (!PhoneValidator.IsValid(phone))
            throw new DomainRuleException(ResourcesMessageException.INVALID_PHONE);
    }
}
