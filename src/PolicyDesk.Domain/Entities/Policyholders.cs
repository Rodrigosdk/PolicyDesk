using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;

namespace PolicyDesk.Domain.Entities;

public class Policyholders(
    string name, 
    string cpf, 
    DateTime birthDate, 
    string email,
    string phone) : Entity
{
    public string Name { get; private set; } = NormalizeNameOrThrow(name);
    public string Cpf { get; private set; } = CpfValidator.NormalizeOrThrow(cpf);
    public DateTime BirthDate { get; private set; } = birthDate;
    public string Email { get; private set; } = EmailValidator.NormalizeOrThrow(email);
    public string Phone {get; private set; } = phone;

    public void Update(string name, DateTime birthDate, string email, string phone)
    {
        var normalizedName = NormalizeNameOrThrow(name);
        var normalizedEmail = EmailValidator.NormalizeOrThrow(email);

        Name = normalizedName;
        BirthDate = birthDate;
        Email = normalizedEmail;
        Phone = phone;
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
            ? throw new ArgumentException(ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED, nameof(name))
            : name.Trim();
    }

}