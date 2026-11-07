using System.Globalization;
using PolicyDesk.Exception;

namespace PolicyDesk.Domain.Validations;

public static class MoneyLimits
{
    public const int Precision = 18;
    public const int Scale = 2;
    public const int IntegerDigits = Precision - Scale;

    private static readonly decimal MaxExclusive = (decimal)Math.Pow(10, IntegerDigits);

    public static bool IsValid(decimal amount) =>
        decimal.Round(amount, Scale) == amount && Math.Abs(amount) < MaxExclusive;

    public static string CoverageAmountInvalidPrecision => Format(ResourcesMessageException.COVERAGE_AMOUNT_INVALID_PRECISION);

    public static string PremiumAmountInvalidPrecision => Format(ResourcesMessageException.PREMIUM_AMOUNT_INVALID_PRECISION);

    public static string ClaimAmountInvalidPrecision => Format(ResourcesMessageException.CLAIM_AMOUNT_INVALID_PRECISION);

    private static string Format(string message) =>
        string.Format(CultureInfo.CurrentCulture, message, IntegerDigits, Scale);
}
