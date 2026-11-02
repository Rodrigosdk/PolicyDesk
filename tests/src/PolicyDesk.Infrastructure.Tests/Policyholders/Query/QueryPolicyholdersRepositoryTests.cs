using CommonTestUtilities.Entities;
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

    [Theory(DisplayName = nameof(Deve_ConsiderarApolice_Conforme_Vigencia))]
    [Trait("Infrastructure", "PolicyholdersRepository - Consulta")]
    [InlineData(-1, 1, true)]     // vigente hoje
    [InlineData(10, 40, true)]    // vigência futura: também impede a inativação
    [InlineData(-30, 0, true)]    // termina hoje: ainda vale
    [InlineData(-30, -1, false)]  // já terminou
    public async Task Deve_ConsiderarApolice_Conforme_Vigencia(int startOffset, int endOffset, bool expected)
    {
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var policyholder = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivate(policyholder);
        var today = DateTime.UtcNow.Date;
        dbContext.Policies.Add(PolicyBuilder.Build(
            policyholder.Id, startDate: today.AddDays(startOffset), endDate: today.AddDays(endOffset)));
        await dbContext.SaveChangesAsync();

        (await repository.HasActiveOrFuturePolicy(policyholder.Id)).ShouldBe(expected);
    }

    [Fact(DisplayName = nameof(Deve_IgnorarApoliceCancelada_Quando_VerificarApoliceAtivaOuFutura))]
    [Trait("Infrastructure", "PolicyholdersRepository - Consulta")]
    public async Task Deve_IgnorarApoliceCancelada_Quando_VerificarApoliceAtivaOuFutura()
    {
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var policyholder = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivate(policyholder);
        var today = DateTime.UtcNow.Date;
        var futurePolicy = PolicyBuilder.Build(policyholder.Id, startDate: today.AddDays(10), endDate: today.AddDays(40));
        futurePolicy.Cancel();
        dbContext.Policies.Add(futurePolicy);
        await dbContext.SaveChangesAsync();

        (await repository.HasActiveOrFuturePolicy(policyholder.Id)).ShouldBeFalse();
    }
}
