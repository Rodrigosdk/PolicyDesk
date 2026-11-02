using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Domain.Entities;

public class Policyholders : Entity
{
    public Policyholders(string name, string cpf, DateTime birthDate, string email, string phone)
    {
        Name = NormalizeNameOrThrow(name);
        Cpf = CpfValidator.NormalizeOrThrow(cpf);
        BirthDate = EnsureValidBirthDate(birthDate);
        Email = EmailValidator.NormalizeOrThrow(email);
        Phone = EnsureValidPhone(phone);
    }

    // Usado pelo EF Core ao carregar do banco: não reaplica as regras de criação,
    // para que registros gravados antes de uma regra nova continuem legíveis
    private Policyholders()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public DateTime BirthDate { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public bool Active { get; private set; } = true;

    public void Update(string name, DateTime birthDate, string email, string phone)
    {
        // Valida tudo antes de alterar qualquer campo
        var normalizedName = NormalizeNameOrThrow(name);
        var validBirthDate = EnsureValidBirthDate(birthDate);
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
        return string.IsNullOrWhiteSpace(name)
            ? throw new DomainRuleException(ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED)
            : name.Trim();
    }

    private static DateTime EnsureValidBirthDate(DateTime birthDate)
    {
        if (birthDate == default)
            throw new DomainRuleException(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_REQUIRED);

        return birthDate.Date > DateTime.UtcNow.Date
            ? throw new DomainRuleException(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_IN_FUTURE)
            : birthDate;
    }

    private static string EnsureValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainRuleException(ResourcesMessageException.POLICYHOLDER_PHONE_REQUIRED);

        return PhoneValidator.IsValid(phone)
            ? phone
            : throw new DomainRuleException(ResourcesMessageException.INVALID_PHONE);
    }
}
