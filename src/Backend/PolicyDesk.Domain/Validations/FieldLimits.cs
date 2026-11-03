using PolicyDesk.Exception;

namespace PolicyDesk.Domain.Validations;

public static class FieldLimits
{
    public const int Cpf = 11;
    public const int Email = 254;
    public const int PolicyholderName = 150;
    public const int Phone = 20;
    public const int Description = 500;
    public const int DenialReason = 1000;

    public static bool Exceeds(string? value, int maxLength) =>
        value is not null && value.Trim().Length > maxLength;

    public static string PolicyholderNameTooLong =>
        string.Format(ResourcesMessageException.POLICYHOLDER_NAME_TOO_LONG, PolicyholderName);

    public static string DescriptionTooLong =>
        string.Format(ResourcesMessageException.DESCRIPTION_TOO_LONG, Description);

    public static string DenialReasonTooLong =>
        string.Format(ResourcesMessageException.CLAIM_DENIAL_REASON_TOO_LONG, DenialReason);
}
