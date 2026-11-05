using Microsoft.EntityFrameworkCore;
using PolicyDesk.Infrastructure.DataAccess;
using PolicyDesk.Infrastructure.DataAccess.Repositories;
using Policyholder = PolicyDesk.Domain.Entities.Policyholders;

namespace PolicyDesk.Infrastructure.Tests.Policyholders;

public abstract class PolicyholdersRepositoryTestBase(PostgreSqlInfrastructureFixture fixture)
    : IClassFixture<PostgreSqlInfrastructureFixture>
{
    protected PostgreSqlInfrastructureFixture Fixture { get; } = fixture;

    internal static PolicyholdersRepository CreateRepository(PolicyDeskDbContext dbContext) => new(dbContext);

    // O registro usa SQL direto, então a entidade só fica rastreada ao ser carregada do banco
    internal static async Task DeactivateAndCommit(PolicyDeskDbContext dbContext, Policyholder policyholder)
    {
        var tracked = await dbContext.Policyholders.SingleAsync(item => item.Id == policyholder.Id);
        tracked.Deactivate();
        await new UnitOfWork(dbContext).Commit();
    }
}
