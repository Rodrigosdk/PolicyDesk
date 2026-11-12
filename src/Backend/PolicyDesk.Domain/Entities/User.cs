using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Domain.Entities;

public class User : Entity
{
    public User(string name, string email, string password)
    {
        Name = NormalizeNameOrThrow(name);
        Email = EmailValidator.NormalizeOrThrow(email);
        Password = EnsureValidPassword(password);
    }

    private User()
    {
    }

    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public bool Active { get; private set; } = true;

    public void Update(string name, string email, string password)
    {
        var normalizedName = NormalizeNameOrThrow(name);
        var normalizedEmail = EmailValidator.NormalizeOrThrow(email);
        var validPassword = EnsureValidPassword(password);

        Name = normalizedName;
        Email = normalizedEmail;
        Password = validPassword;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Active = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        Active = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeNameOrThrow(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainRuleException(ResourcesMessageException.USER_NAME_REQUIRED);

        return FieldLimits.Exceeds(name, FieldLimits.UserName)
            ? throw new DomainRuleException(FieldLimits.UserNameTooLong)
            : name.Trim();
    }

    private static string EnsureValidPassword(string? password)
    {
        return string.IsNullOrWhiteSpace(password)
            ? throw new DomainRuleException(ResourcesMessageException.USER_PASSWORD_REQUIRED)
            : password;
    }
}
