using PolicyDesk.Exception;

namespace PolicyDesk.Domain.Validations;

public static class CpfValidator
{
    private const int CpfLength = 11;

    public static string Normalize(string? cpf)
    {
        return string.IsNullOrWhiteSpace(cpf) 
            ? string.Empty 
            : new string(cpf.Where(char.IsAsciiDigit).ToArray());
    }

    public static bool IsValid(string? cpf)
    {
        var digits = Normalize(cpf);

        if (digits.Length != CpfLength || digits.Distinct().Count() == 1)
            return false;

        return digits[9] - '0' == CalculateCheckDigit(digits, 9)
               && digits[10] - '0' == CalculateCheckDigit(digits, 10);
    }

    // Normaliza e valida, lançando exceção se o CPF for inválido
    public static string NormalizeOrThrow(string? cpf)
    {
        var digits = Normalize(cpf);

        return !IsValid(digits) 
            ? throw new ArgumentException(ResourcesMessageException.INVALID_CPF, nameof(cpf))
            : digits;
    }

    public static string Format(string cpf)
    {
        var digits = Normalize(cpf);

        return digits.Length == CpfLength
            ? $"{digits[..3]}.{digits[3..6]}.{digits[6..9]}-{digits[9..]}"
            : cpf;
    }

    private static int CalculateCheckDigit(string digits, int length)
    {
        var sum = 0;
        for (var i = 0; i < length; i++)
            sum += (digits[i] - '0') * (length + 1 - i);

        var remainder = sum * 10 % 11;
        return remainder == 10 ? 0 : remainder;
    }
}