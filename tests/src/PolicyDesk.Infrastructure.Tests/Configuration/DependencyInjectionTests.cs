using CommonTestUtilities.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Exception;
using PolicyDesk.Infrastructure.Configuration;
using PolicyDesk.Infrastructure.DataAccess;
using Shouldly;

namespace PolicyDesk.Infrastructure.Tests.Configuration;

public class DependencyInjectionTests
{
    [Theory(DisplayName = nameof(Deve_FalharNaInicializacaoComMensagemExplicativa_Quando_ConnectionStringNaoForConfigurada))]
    [Trait("Infrastructure", "Configuração - Connection string")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_FalharNaInicializacaoComMensagemExplicativa_Quando_ConnectionStringNaoForConfigurada(string? connectionString)
    {
        using var provider = BuildProvider(connectionString);

        var exception = Should.Throw<OptionsValidationException>(
            () => provider.GetRequiredService<IStartupValidator>().Validate());

        exception.Message.ShouldContain("ConnectionStrings:DbConnection");
        exception.Message.ShouldContain("ConnectionStrings__DbConnection");
    }

    [Fact(DisplayName = nameof(Deve_LancarMensagemExplicativa_Quando_DbContextForCriadoSemConnectionString))]
    [Trait("Infrastructure", "Configuração - Connection string")]
    public void Deve_LancarMensagemExplicativa_Quando_DbContextForCriadoSemConnectionString()
    {
        using var provider = BuildProvider(null);
        using var scope = provider.CreateScope();

        var exception = Should.Throw<OptionsValidationException>(
            () => scope.ServiceProvider.GetRequiredService<PolicyDeskDbContext>());

        exception.Message.ShouldContain("ConnectionStrings__DbConnection");
    }

    [Fact(DisplayName = nameof(Deve_PassarNaValidacaoDeInicializacao_Quando_ConnectionStringForConfigurada))]
    [Trait("Infrastructure", "Configuração - Connection string")]
    public void Deve_PassarNaValidacaoDeInicializacao_Quando_ConnectionStringForConfigurada()
    {
        using var provider = BuildProvider("Host=localhost;Database=PolicyDesk");

        Should.NotThrow(() => provider.GetRequiredService<IStartupValidator>().Validate());
    }

    [Theory(DisplayName = nameof(Deve_FalharNaInicializacao_Quando_FusoHorarioDeNegocioNaoForConfiguradoOuForInvalido))]
    [Trait("Infrastructure", "Configuração - Fuso horário")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("America/Inexistente")]
    public void Deve_FalharNaInicializacao_Quando_FusoHorarioDeNegocioNaoForConfiguradoOuForInvalido(string? timeZone)
    {
        using var provider = BuildProvider("Host=localhost;Database=PolicyDesk", timeZone);

        var exception = Should.Throw<OptionsValidationException>(
            () => provider.GetRequiredService<IStartupValidator>().Validate());

        exception.Message.ShouldContain(ResourcesMessageException.INVALID_BUSINESS_TIME_ZONE);
    }

    [Fact(DisplayName = nameof(Deve_CalcularODiaNoFusoConfigurado_Quando_FusoHorarioForValido))]
    [Trait("Infrastructure", "Configuração - Fuso horário")]
    public void Deve_CalcularODiaNoFusoConfigurado_Quando_FusoHorarioForValido()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 3, 11, 1, 0, 0, TimeSpan.Zero));
        using var provider = BuildProvider("Host=localhost;Database=PolicyDesk", configureServices: services => services.AddSingleton<TimeProvider>(timeProvider));

        var clock = provider.GetRequiredService<BusinessClock>();

        clock.Today.ShouldBe(new DateOnly(2026, 3, 10));
        clock.UtcNow.ShouldBe(new DateTime(2026, 3, 11, 1, 0, 0, DateTimeKind.Utc));
    }

    [Theory(DisplayName = nameof(Deve_FalharNaInicializacao_Quando_ChaveDoJwtForAusenteOuCurta))]
    [Trait("Infrastructure", "Configuração - Jwt")]
    [InlineData("")]
    [InlineData("chave-curta")]
    [InlineData("1234567890123456789012345678901")]
    public void Deve_FalharNaInicializacao_Quando_ChaveDoJwtForAusenteOuCurta(string signingKey)
    {
        using var provider = BuildProvider("Host=localhost;Database=PolicyDesk", jwtOptions: JwtOptionsBuilder.Build(signingKey));

        var exception = Should.Throw<OptionsValidationException>(
            () => provider.GetRequiredService<IStartupValidator>().Validate());

        exception.Message.ShouldContain("Jwt__SigningKey");
    }

    [Fact(DisplayName = nameof(Deve_PassarNaValidacaoDeInicializacao_Quando_ChaveDoJwtTiverTamanhoMinimo))]
    [Trait("Infrastructure", "Configuração - Jwt")]
    public void Deve_PassarNaValidacaoDeInicializacao_Quando_ChaveDoJwtTiverTamanhoMinimo()
    {
        var signingKey = new string('a', JwtOptions.MinimumSigningKeyBytes);
        using var provider = BuildProvider("Host=localhost;Database=PolicyDesk", jwtOptions: JwtOptionsBuilder.Build(signingKey));

        Should.NotThrow(() => provider.GetRequiredService<IStartupValidator>().Validate());
    }

    private static ServiceProvider BuildProvider(
        string? connectionString,
        string? timeZone = "America/Sao_Paulo",
        Action<IServiceCollection>? configureServices = null,
        JwtOptions? jwtOptions = null)
    {
        var settings = JwtOptionsBuilder.Settings(jwtOptions ?? JwtOptionsBuilder.Build());
        settings["ConnectionStrings:DbConnection"] = connectionString;
        if (timeZone is not null)
            settings["Business:TimeZone"] = timeZone;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        var services = new ServiceCollection();
        configureServices?.Invoke(services);
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider();
    }
}
