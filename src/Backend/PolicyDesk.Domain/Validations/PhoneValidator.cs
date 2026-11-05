using System.Diagnostics.CodeAnalysis;
using PhoneNumbers;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Domain.Validations;

public static class PhoneValidator
{
    private const string DefaultRegion = "BR";

    private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();

    public static bool IsValid(string? phone) => TryParse(phone, out _);

    public static string NormalizeOrThrow(string? phone)
    {
        return TryParse(phone, out var number)
            ? PhoneUtil.Format(number, PhoneNumberFormat.E164)
            : throw new DomainRuleException(ResourcesMessageException.INVALID_PHONE);
    }

    private static bool TryParse(string? phone, [NotNullWhen(true)] out PhoneNumber? number)
    {
        number = null;

        if (string.IsNullOrWhiteSpace(phone) || phone.Length > FieldLimits.Phone)
            return false;

        if (phone.Any(character => char.IsDigit(character) && !char.IsAsciiDigit(character)))
            return false;

        try
        {
            var parsed = PhoneUtil.Parse(phone, DefaultRegion);
            if (parsed.HasExtension || !PhoneUtil.IsValidNumber(parsed))
                return false;

            number = parsed;
            return true;
        }
        catch (NumberParseException)
        {
            return false;
        }
    }
}
