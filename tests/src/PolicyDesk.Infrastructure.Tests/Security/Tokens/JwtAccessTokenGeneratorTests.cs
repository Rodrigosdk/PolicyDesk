using CommonTestUtilities.Security;
using CommonTestUtilities.Time;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Infrastructure.Configuration;
using PolicyDesk.Infrastructure.Security.Tokens;
using Shouldly;

namespace PolicyDesk.Infrastructure.Tests.Security.Tokens;

public class JwtAccessTokenGeneratorTests
{
    [Fact(DisplayName = nameof(Deve_GerarTokenValidoComOUsuario_Quando_Gerar))]
    [Trait("Infrastructure", "JwtAccessTokenGenerator - Generate")]
    public async Task Deve_GerarTokenValidoComOUsuario_Quando_Gerar()
    {
        var options = JwtOptionsBuilder.Build();
        var userId = Guid.NewGuid();

        var accessToken = CreateGenerator(options).Generate(userId);

        var result = await ValidateAsync(accessToken.Token, options);
        result.IsValid.ShouldBeTrue();
        result.Claims[JwtRegisteredClaimNames.Sub].ShouldBe(userId.ToString());
        result.Claims.ShouldContainKey(JwtRegisteredClaimNames.Jti);
    }

    [Fact(DisplayName = nameof(Deve_ExpirarNoPrazoConfigurado_Quando_Gerar))]
    [Trait("Infrastructure", "JwtAccessTokenGenerator - Generate")]
    public void Deve_ExpirarNoPrazoConfigurado_Quando_Gerar()
    {
        var options = JwtOptionsBuilder.Build();
        var clock = BusinessClockBuilder.Build(DateTimeOffset.UtcNow);

        var accessToken = CreateGenerator(options, clock).Generate(Guid.NewGuid());

        var expectedExpiration = clock.UtcNow.AddMinutes(options.ExpirationMinutes);
        accessToken.ExpiresAt.ShouldBe(expectedExpiration);
        new JsonWebToken(accessToken.Token).ValidTo.ShouldBe(expectedExpiration, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = nameof(Deve_GerarTokensDiferentes_Quando_GerarDuasVezesParaOMesmoUsuario))]
    [Trait("Infrastructure", "JwtAccessTokenGenerator - Generate")]
    public void Deve_GerarTokensDiferentes_Quando_GerarDuasVezesParaOMesmoUsuario()
    {
        var generator = CreateGenerator(JwtOptionsBuilder.Build());
        var userId = Guid.NewGuid();

        var first = generator.Generate(userId);
        var second = generator.Generate(userId);

        first.Token.ShouldNotBe(second.Token);
    }

    [Fact(DisplayName = nameof(Deve_RecusarToken_Quando_ValidarComOutraChave))]
    [Trait("Infrastructure", "JwtAccessTokenGenerator - Generate")]
    public async Task Deve_RecusarToken_Quando_ValidarComOutraChave()
    {
        var accessToken = CreateGenerator(JwtOptionsBuilder.Build()).Generate(Guid.NewGuid());

        var result = await ValidateAsync(accessToken.Token, JwtOptionsBuilder.Build());

        result.IsValid.ShouldBeFalse();
    }

    private static JwtAccessTokenGenerator CreateGenerator(JwtOptions options, BusinessClock? clock = null) =>
        new(Options.Create(options), clock ?? BusinessClockBuilder.Build(DateTimeOffset.UtcNow));

    private static Task<TokenValidationResult> ValidateAsync(string token, JwtOptions options) =>
        new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = options.Issuer,
            ValidAudience = options.Audience,
            IssuerSigningKey = options.CreateSigningKey()
        });
}
