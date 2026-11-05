using PolicyDesk.Infrastructure.DataAccess;
using PolicyDesk.Infrastructure.DataAccess.Repositories;
using PolicyDesk.Infrastructure.Tests.Policyholders;

namespace PolicyDesk.Infrastructure.Tests.Claims;

public abstract class ClaimsRepositoryTestBase(PostgreSqlInfrastructureFixture fixture)
    : IClassFixture<PostgreSqlInfrastructureFixture>
{
    protected PostgreSqlInfrastructureFixture Fixture { get; } = fixture;

    internal static ClaimsRepository CreateRepository(PolicyDeskDbContext dbContext) => new(dbContext);
}
