using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolicyDesk.Infrastructure.DataAccess;
using Shouldly;

namespace PolicyDesk.Infrastructure.Tests.Configuration;

public class DependencyInjectionTests
{
    [Theory(DisplayName = nameof(Deve_LancarExcecaoExplicativa_Quando_ConnectionStringNaoForConfigurada))]
    [Trait("Infrastructure", "Configuração - Connection string")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_LancarExcecaoExplicativa_Quando_ConnectionStringNaoForConfigurada(string? connectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DbConnection"] = connectionString
            })
            .Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var exception = Should.Throw<InvalidOperationException>(
            () => scope.ServiceProvider.GetRequiredService<PolicyDeskDbContext>());

        exception.Message.ShouldContain("ConnectionStrings:DbConnection");
        exception.Message.ShouldContain("ConnectionStrings__DbConnection");
    }
}
