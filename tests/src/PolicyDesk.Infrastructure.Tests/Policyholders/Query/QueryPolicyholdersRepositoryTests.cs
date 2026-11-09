using CommonTestUtilities.Entities;
using CommonTestUtilities.Time;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Infrastructure.DataAccess;
using Shouldly;
using PolicyDesk.Domain.Entities;

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
        await repository.RegisterOrReactivateAsync(active);
        await repository.RegisterOrReactivateAsync(inactive);
        await DeactivateAndCommit(dbContext, inactive);

        var listed = (await repository.GetPageAsync(1, 100)).Items;
        var inactiveById = await repository.GetByIdAsync(inactive.Id);

        listed.Select(item => item.Id).ShouldBe([active.Id]);
        inactiveById.ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_CarregarRegistroLegado_Quando_DadosNaoAtendemRegrasAtuais))]
    [Trait("Infrastructure", "PolicyholdersRepository - Consulta")]
    public async Task Deve_CarregarRegistroLegado_Quando_DadosNaoAtendemRegrasAtuais()
    {
        // Registro gravado antes das regras atuais: e-mail sem domínio, telefone curto, nascimento padrão.
        // As regras valem para criar e alterar; carregar do banco não pode falhar.
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var id = Guid.NewGuid();
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO policyholders (id, name, cpf, birth_date, email, phone, created_at, updated_at, active)
            VALUES ({id}, '', '00000000000', '0001-01-01', 'legacy', '123', {DateTime.UtcNow}, NULL, TRUE)
            """);

        var listed = (await CreateRepository(dbContext).GetPageAsync(1, 100)).Items;
        var byId = await CreateRepository(dbContext).GetByIdAsync(id);

        listed.Select(item => item.Id).ShouldBe([id]);
        byId.ShouldNotBeNull();
        byId.Email.ShouldBe("legacy");
    }

    [Fact(DisplayName = nameof(Deve_RetornarSeguradoInativo_Quando_BuscarIncluindoInativos))]
    [Trait("Infrastructure", "PolicyholdersRepository - Consulta")]
    public async Task Deve_RetornarSeguradoInativo_Quando_BuscarIncluindoInativos()
    {
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var inactive = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivateAsync(inactive);
        await DeactivateAndCommit(dbContext, inactive);

        var found = await repository.GetByIdIncludingInactiveAsync(inactive.Id);
        var missing = await repository.GetByIdIncludingInactiveAsync(Guid.NewGuid());

        found.ShouldNotBeNull();
        found.Active.ShouldBeFalse();
        missing.ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_RastrearSegurado_Quando_BuscarIncluindoInativos))]
    [Trait("Infrastructure", "PolicyholdersRepository - Consulta")]
    public async Task Deve_RastrearSegurado_Quando_BuscarIncluindoInativos()
    {
        // O cadastro de apólice altera o segurado retornado; se a consulta não fosse rastreada,
        // a alteração não seria gravada e a proteção contra inativação concorrente (A13) se perderia
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var policyholder = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivateAsync(policyholder);

        var found = await repository.GetByIdIncludingInactiveAsync(policyholder.Id);
        found.ShouldNotBeNull();
        found.RegisterPolicy();
        await new UnitOfWork(dbContext).CommitAsync();

        await using var verificationContext = Fixture.CreateDbContext();
        (await verificationContext.Policyholders.IgnoreQueryFilters().AsNoTracking().SingleAsync(p => p.Id == policyholder.Id))
            .UpdatedAt.ShouldNotBeNull();
    }

    [Fact(DisplayName = nameof(Deve_IgnorarSeguradoAtual_Quando_VerificarEmailExistente))]
    [Trait("Infrastructure", "PolicyholdersRepository - Consulta")]
    public async Task Deve_IgnorarSeguradoAtual_Quando_VerificarEmailExistente()
    {
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var policyholder = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivateAsync(policyholder);

        var exists = await repository.ExistActivePolicyholdersWithEmailAsync(policyholder.Email);
        var existsExceptSelf = await repository.ExistActivePolicyholdersWithEmailAsync(policyholder.Email, policyholder.Id);

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
        await repository.RegisterOrReactivateAsync(policyholder);
        var today = BusinessClockBuilder.DefaultToday;
        dbContext.Policies.Add(PolicyBuilder.Build(
            policyholder.Id, startDate: today.AddDays(startOffset), endDate: today.AddDays(endOffset)));
        await dbContext.SaveChangesAsync();

        (await repository.HasActiveOrFuturePolicyAsync(policyholder.Id, BusinessClockBuilder.DefaultToday)).ShouldBe(expected);
    }

    [Fact(DisplayName = nameof(Deve_IgnorarApoliceCancelada_Quando_VerificarApoliceAtivaOuFutura))]
    [Trait("Infrastructure", "PolicyholdersRepository - Consulta")]
    public async Task Deve_IgnorarApoliceCancelada_Quando_VerificarApoliceAtivaOuFutura()
    {
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var policyholder = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivateAsync(policyholder);
        var today = BusinessClockBuilder.DefaultToday;
        var futurePolicy = PolicyBuilder.Build(policyholder.Id, startDate: today.AddDays(10), endDate: today.AddDays(40));
        futurePolicy.Cancel();
        dbContext.Policies.Add(futurePolicy);
        await dbContext.SaveChangesAsync();

        (await repository.HasActiveOrFuturePolicyAsync(policyholder.Id, BusinessClockBuilder.DefaultToday)).ShouldBeFalse();
    }


    [Fact(DisplayName = nameof(Deve_IgnorarInativos_Quando_ConsultarSeguradosEmQualquerConsultaLinq))]
    [Trait("Infrastructure", "PolicyholdersRepository - Filtro global")]
    public async Task Deve_IgnorarInativos_Quando_ConsultarSeguradosEmQualquerConsultaLinq()
    {
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var active = PolicyholderBuilder.Build();
        var inactive = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivateAsync(active);
        await repository.RegisterOrReactivateAsync(inactive);
        await DeactivateAndCommit(dbContext, inactive);

        await using var queryContext = Fixture.CreateDbContext();
        var byCpf = await queryContext.Policyholders.SingleOrDefaultAsync(p => p.Cpf == inactive.Cpf);
        var count = await queryContext.Policyholders.CountAsync();
        var withoutFilter = await queryContext.Policyholders.IgnoreQueryFilters().CountAsync();

        byCpf.ShouldBeNull();
        count.ShouldBe(1);
        withoutFilter.ShouldBe(2);
    }


    [Fact(DisplayName = nameof(Deve_RastrearSomenteAtivos_Quando_BuscarSeguradoParaAlteracao))]
    [Trait("Infrastructure", "PolicyholdersRepository - Rastreamento")]
    public async Task Deve_RastrearSomenteAtivos_Quando_BuscarSeguradoParaAlteracao()
    {
        await using var dbContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(dbContext);
        var active = PolicyholderBuilder.Build();
        var inactive = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivateAsync(active);
        await repository.RegisterOrReactivateAsync(inactive);
        await DeactivateAndCommit(dbContext, inactive);

        await using var readContext = Fixture.CreateDbContext();
        (await CreateRepository(readContext).GetByIdAsync(active.Id)).ShouldNotBeNull();
        readContext.ChangeTracker.Entries<Policyholder>().ShouldBeEmpty();

        await using var updateContext = Fixture.CreateDbContext();
        var updateRepository = CreateRepository(updateContext);
        (await updateRepository.GetByIdForUpdateAsync(active.Id)).ShouldNotBeNull();
        (await updateRepository.GetByIdForUpdateAsync(inactive.Id)).ShouldBeNull();
        updateContext.ChangeTracker.Entries<Policyholder>().ShouldHaveSingleItem();
    }
}
