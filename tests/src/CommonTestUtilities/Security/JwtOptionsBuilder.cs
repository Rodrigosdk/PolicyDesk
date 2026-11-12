using System.Globalization;
using Bogus;
using PolicyDesk.Infrastructure.Configuration;

namespace CommonTestUtilities.Security;

public static class JwtOptionsBuilder
{
    public const string Issuer = "PolicyDesk.Tests";
    public const string Audience = "PolicyDesk.Tests";
    public const int ExpirationMinutes = 60;

    public static JwtOptions Build(string? signingKey = null) => new()
    {
        Issuer = Issuer,
        Audience = Audience,
        SigningKey = signingKey ?? new Faker().Random.AlphaNumeric(64),
        ExpirationMinutes = ExpirationMinutes
    };

    public static Dictionary<string, string?> Settings(JwtOptions options) => new()
    {
        [$"{JwtOptions.SectionName}:{nameof(JwtOptions.Issuer)}"] = options.Issuer,
        [$"{JwtOptions.SectionName}:{nameof(JwtOptions.Audience)}"] = options.Audience,
        [$"{JwtOptions.SectionName}:{nameof(JwtOptions.SigningKey)}"] = options.SigningKey,
        [$"{JwtOptions.SectionName}:{nameof(JwtOptions.ExpirationMinutes)}"] = options.ExpirationMinutes.ToString(CultureInfo.InvariantCulture)
    };
}
