namespace PolicyDesk.Domain.Validations;

public static class PhoneValidator
{
    // Sem "+": número brasileiro local, DDD + número, com 10 dígitos (fixo) ou 11 (celular)
    private const int MinLocalDigits = 10;
    private const int MaxLocalDigits = 11;

    // Com "+": formato internacional E.164, código do país + número, de 7 a 15 dígitos no total
    private const int MinInternationalDigits = 7;
    private const int MaxInternationalDigits = 15;

    // Para o Brasil a validação continua estrita: após o 55 vem um número local de 10 ou 11 dígitos.
    // Códigos de país não são prefixo uns dos outros, então nenhum outro país começa com 55.
    private const string BrazilCountryCode = "55";

    public static bool IsValid(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length > FieldLimits.Phone)
            return false;

        var digits = new string(phone.Where(char.IsAsciiDigit).ToArray());

        return phone.TrimStart().StartsWith('+')
            ? IsInternationalNumber(digits)
            : IsLocalNumber(digits);
    }

    private static bool IsInternationalNumber(string digits)
    {
        if (digits.StartsWith(BrazilCountryCode, StringComparison.Ordinal))
            return IsLocalNumber(digits[BrazilCountryCode.Length..]);

        // Nenhum código de país começa com 0
        return digits.Length is >= MinInternationalDigits and <= MaxInternationalDigits
               && digits[0] != '0';
    }

    private static bool IsLocalNumber(string digits) =>
        digits.Length is >= MinLocalDigits and <= MaxLocalDigits;
}
