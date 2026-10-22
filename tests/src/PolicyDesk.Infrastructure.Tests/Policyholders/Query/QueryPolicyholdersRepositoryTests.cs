using CommonTestUtilities.Entities;
using PolicyDesk.Domain.Entities;
using Shouldly;

namespace PolicyDesk.Infrastructure.Tests.Policyholders.Query;

public class QueryPolicyholdersRepositoryTests(PostgreSqlInfrastructureFixture fixture)
    : PolicyholdersRepositoryTestBase(fixture)
{
    [Fact(DisplayName = nameof(Deve_RetornarSomenteAtivos_Quando_ListarOuBuscarPorId))]
    [Trait("Infrastructure", "PolicyholdersRepository - Consulta")]
    public async Task Deve_RetornarSomenteAtivos_Quando_ListarOuBuscarPorId()
    {
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var active = PolicyholderBuilder.Build();
        var inactive = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivate(active);
        await repository.RegisterOrReactivate(inactive);
        await DeactivateAndCommit(dbContext, inactive);

        var listed = await repository.GetAll();
        var inactiveById = await repository.GetById(inactive.Id);

        listed.Select(item => item.Id).ShouldBe([active.Id]);
        inactiveById.ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_IgnorarSeguradoAtual_Quando_VerificarEmailExistente))]
    [Trait("Infrastructure", "PolicyholdersRepository - Consulta")]
    public async Task Deve_IgnorarSeguradoAtual_Quando_VerificarEmailExistente()
    {
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var policyholder = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivate(policyholder);

        var exists = await repository.ExistActivePolicyholdersWithEmail(policyholder.Email);
        var existsExceptSelf = await repository.ExistActivePolicyholdersWithEmail(policyholder.Email, policyholder.Id);

        exists.ShouldBeTrue();
        existsExceptSelf.ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_RetornarVerdadeiro_Somente_Quando_ApoliceAtivaEstiverVigente))]
    [Trait("Infrastructure", "PolicyholdersRepository - Consulta")]
    public async Task Deve_RetornarVerdadeiro_Somente_Quando_ApoliceAtivaEstiverVigente()
    {
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var policyholder = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivate(policyholder);
        var today = DateTime.UtcNow.Date;
        var currentPolicy = new Policy(policyholder.Id, "Current policy", 10000, 100, today.AddDays(-1), today.AddDays(1));
        dbContext.Policies.Add(currentPolicy);
        await dbContext.SaveChangesAsync();

        (await repository.HasActivePolicy(policyholder.Id)).ShouldBeTrue();

        currentPolicy.Cancel();
        await dbContext.SaveChangesAsync();

        (await repository.HasActivePolicy(policyholder.Id)).ShouldBeFalse();
    }
}
