using System.Globalization;
using CommonTestUtilities.Time;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Testcontainers.PostgreSql;

namespace PolicyDesk.Api.IntegrationTests;

public class PolicyDeskApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    static PolicyDeskApplicationFactory()
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");
    }

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("policydesk_integration_tests")
        .WithUsername("policydesk")
        .WithPassword("test-password")
        .Build();

    public FakeTimeProvider Clock { get; } = new(BusinessClockBuilder.DefaultUtcNow);

    public async Task InitializeAsync() => await _database.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DbConnection"] = _database.GetConnectionString()
            }));
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }
}
