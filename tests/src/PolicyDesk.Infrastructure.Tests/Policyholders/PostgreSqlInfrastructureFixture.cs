using Microsoft.EntityFrameworkCore;
using PolicyDesk.Infrastructure.DataAccess;
using Testcontainers.PostgreSql;

namespace PolicyDesk.Infrastructure.Tests.Policyholders;

public sealed class PostgreSqlInfrastructureFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("policydesk_infrastructure_tests")
        .WithUsername("policydesk")
        .WithPassword("test-password")
        .Build();

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _database.DisposeAsync();

    internal PolicyDeskDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<PolicyDeskDbContext>()
            .UseNpgsql(_database.GetConnectionString())
            .Options;

        return new PolicyDeskDbContext(options);
    }

    internal async Task<PolicyDeskDbContext> CreateCleanDbContextAsync()
    {
        var dbContext = CreateDbContext();
        await dbContext.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE claims, policies, policyholders CASCADE;");

        return dbContext;
    }
}
