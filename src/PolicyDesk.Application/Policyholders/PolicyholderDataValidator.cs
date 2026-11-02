using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;

namespace PolicyDesk.Application.Policyholders;

// Regras comuns ao cadastro e à atualização de segurado
internal static class PolicyholderDataValidator
{
    private const int MinPhoneDigits = 10;
    private const int MaxPhoneDigits = 11;

    public static List<string> Validate(string? name, DateTime birthDate, string? email, string? phone)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED);

        if (birthDate == default)
            errors.Add(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_REQUIRED);
        else if (birthDate.Date > DateTime.UtcNow.Date)
            errors.Add(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_IN_FUTURE);

        if (!EmailValidator.IsValid(email))
            errors.Add(ResourcesMessageException.INVALID_EMAIL);

        if (string.IsNullOrWhiteSpace(phone))
            errors.Add(ResourcesMessageException.POLICYHOLDER_PHONE_REQUIRED);
        else if (phone.Count(char.IsAsciiDigit) is < MinPhoneDigits or > MaxPhoneDigits)
            errors.Add(ResourcesMessageException.INVALID_PHONE);

        return errors;
    }
}
