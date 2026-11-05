using PolicyDesk.Infrastructure.DataAccess;
using PolicyDesk.Infrastructure.DataAccess.Repositories;
using PolicyDesk.Infrastructure.Tests.Policyholders;

namespace PolicyDesk.Infrastructure.Tests.Policies;

public abstract class PoliciesRepositoryTestBase(PostgreSqlInfrastructureFixture fixture)
    : IClassFixture<PostgreSqlInfrastructureFixture>
{
    protected PostgreSqlInfrastructureFixture Fixture { get; } = fixture;

    internal static PoliciesRepository CreateRepository(PolicyDeskDbContext dbContext) => new(dbContext);
}
