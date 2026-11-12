using PolicyDesk.Infrastructure.DataAccess;
using PolicyDesk.Infrastructure.DataAccess.Repositories;
using PolicyDesk.Infrastructure.Tests.Policyholders;

namespace PolicyDesk.Infrastructure.Tests.Users;

public abstract class UsersRepositoryTestBase(PostgreSqlInfrastructureFixture fixture)
    : IClassFixture<PostgreSqlInfrastructureFixture>
{
    protected PostgreSqlInfrastructureFixture Fixture { get; } = fixture;

    internal static UsersRepository CreateRepository(PolicyDeskDbContext dbContext) => new(dbContext);
}
