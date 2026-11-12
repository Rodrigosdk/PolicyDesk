using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PolicyDesk.Infrastructure.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinimumSigningKeyBytes = 32;

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; }

    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(Issuer) &&
        !string.IsNullOrWhiteSpace(Audience) &&
        Encoding.UTF8.GetByteCount(SigningKey) >= MinimumSigningKeyBytes &&
        ExpirationMinutes > 0;

    public SymmetricSecurityKey CreateSigningKey() => new(Encoding.UTF8.GetBytes(SigningKey));
}
