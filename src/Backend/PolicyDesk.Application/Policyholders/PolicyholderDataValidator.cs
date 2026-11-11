using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;

namespace PolicyDesk.Application.Policyholders;

// Regras comuns ao cadastro e à atualização de segurado
internal static class PolicyholderDataValidator
{
    public static List<string> Validate(string? name, DateOnly birthDate, string? email, string? phone, DateOnly today)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(ResourcesMessageException.POLICYHOLDER_NAME_REQUIRED);
        else if (FieldLimits.Exceeds(name, FieldLimits.PolicyholderName))
            errors.Add(FieldLimits.PolicyholderNameTooLong);

        if (birthDate == default)
            errors.Add(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_REQUIRED);
        else if (birthDate > today)
            errors.Add(ResourcesMessageException.POLICYHOLDER_BIRTH_DATE_IN_FUTURE);

        if (!EmailValidator.IsValid(email))
            errors.Add(ResourcesMessageException.INVALID_EMAIL);

        if (string.IsNullOrWhiteSpace(phone))
            errors.Add(ResourcesMessageException.POLICYHOLDER_PHONE_REQUIRED);
        else if (!PhoneValidator.IsValid(phone))
            errors.Add(ResourcesMessageException.INVALID_PHONE);

        return errors;
    }
}
