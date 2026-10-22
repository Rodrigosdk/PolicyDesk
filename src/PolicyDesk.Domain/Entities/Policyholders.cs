using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Domain.Validations;

namespace PolicyDesk.Domain.Entities;

public class Policyholders(
    string name, 
    string cpf, 
    DateTime birthDate, 
    string email,
    string phone) : Entity
{
    public string Name { get; private set; } = RemoveWhitespace(name);
    public string Cpf { get; private set; } = CpfValidator.NormalizeOrThrow(cpf);
    public DateTime BirthDate { get; private set; } = birthDate;
    public string Email { get; private set; } = EmailValidator.NormalizeOrThrow(email);
    public string Phone {get; private set; } = phone;

    public void Update(string name, DateTime birthDate, string email, string phone)
    {
        var normalizedEmail = EmailValidator.NormalizeOrThrow(email);

        Name = RemoveWhitespace(name);
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
    
    
    private static string RemoveWhitespace(string input)
    {
        return input.Trim();
    }

}