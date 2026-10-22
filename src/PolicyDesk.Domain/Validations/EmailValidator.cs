using System.Net.Mail;
using PolicyDesk.Exception;

namespace PolicyDesk.Domain.Validations;

public static class EmailValidator
{
    private const int MaxLength = 254;

    public static string Normalize(string? email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? string.Empty
            : email.Trim().ToLowerInvariant();
    }

    public static bool IsValid(string? email)
    {
        var normalized = Normalize(email);

        if (normalized.Length is 0 or > MaxLength)
            return false;

        if (!MailAddress.TryCreate(normalized, out var address) || address.Address != normalized)
            return false;

        // Exige domínio com ponto (ex.: user@dominio.com)
        var domain = address.Host;
        return domain.Contains('.') && !domain.StartsWith('.') && !domain.EndsWith('.');
    }

    public static string NormalizeOrThrow(string? email)
    {
        var normalized = Normalize(email);

        return !IsValid(normalized) 
            ? throw new ArgumentException(ResourcesMessageException.INVALID_EMAIL_ARGUMENT, nameof(email))
            : normalized;
    }
}
