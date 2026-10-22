using CommonTestUtilities.Entities;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Infrastructure.DataAccess;
using Shouldly;

namespace PolicyDesk.Infrastructure.Tests.Policyholders.Commit;

public class UnitOfWorkTests(PostgreSqlInfrastructureFixture fixture) : PolicyholdersRepositoryTestBase(fixture)
{
    [Fact(DisplayName = nameof(Deve_PersistirAlteracoes_Quando_ConfirmarUnitOfWork))]
    [Trait("Infrastructure", "UnitOfWork - Commit")]
    public async Task Deve_PersistirAlteracoes_Quando_ConfirmarUnitOfWork()
    {
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var policyholder = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivate(policyholder);
        var tracked = await dbContext.Policyholders.SingleAsync(item => item.Id == policyholder.Id);
        tracked.Deactivate();

        await new UnitOfWork(dbContext).Commit();

        await using var verificationContext = Fixture.CreateDbContext();
        var saved = await verificationContext.Policyholders.SingleAsync(item => item.Id == policyholder.Id);
        saved.Active.ShouldBeFalse();
    }
}
