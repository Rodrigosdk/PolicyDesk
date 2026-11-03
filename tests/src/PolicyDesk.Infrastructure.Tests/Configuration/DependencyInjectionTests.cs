using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

    private static ServiceProvider BuildProvider(string? connectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DbConnection"] = connectionString
            })
            .Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider();
    }
}
