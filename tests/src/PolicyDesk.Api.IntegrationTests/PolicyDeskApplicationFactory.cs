using System.Globalization;
using System.Net.Http.Headers;
using CommonTestUtilities.Security;
using CommonTestUtilities.Time;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Infrastructure.Configuration;
using PolicyDesk.Infrastructure.Security.Tokens;
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

    public JwtOptions Jwt { get; } = JwtOptionsBuilder.Build();

    public string CreateAccessToken(Guid? userId = null, DateTimeOffset? issuedAt = null)
    {
        var clock = new BusinessClock(new FakeTimeProvider(issuedAt ?? Clock.GetUtcNow()), BusinessClockBuilder.TimeZone);
        return new JwtAccessTokenGenerator(Options.Create(Jwt), clock).Generate(userId ?? Guid.NewGuid()).Token;
    }

    public HttpClient Authorize(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateAccessToken());
        return client;
    }

    public HttpClient CreateAnonymousClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = null;
        return client;
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);
        Authorize(client);
    }

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
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DbConnection"] = _database.GetConnectionString()
            });
            configuration.AddInMemoryCollection(JwtOptionsBuilder.Settings(Jwt));
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }
}
