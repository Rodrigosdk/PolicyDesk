using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Domain.Security.Tokens;
using PolicyDesk.Infrastructure.Configuration;

namespace PolicyDesk.Infrastructure.Security.Tokens;

internal sealed class JwtAccessTokenGenerator(IOptions<JwtOptions> options, BusinessClock clock) : IAccessTokenGenerator
{
    private readonly JsonWebTokenHandler _handler = new();

    public AccessToken Generate(Guid userId)
    {
        var jwtOptions = options.Value;
        var issuedAt = clock.UtcNow;
        var expiresAt = issuedAt.AddMinutes(jwtOptions.ExpirationMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ]),
            Issuer = jwtOptions.Issuer,
            Audience = jwtOptions.Audience,
            IssuedAt = issuedAt,
            NotBefore = issuedAt,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(jwtOptions.CreateSigningKey(), SecurityAlgorithms.HmacSha256)
        };

        return new AccessToken(_handler.CreateToken(descriptor), expiresAt);
    }
}
