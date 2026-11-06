using CommonTestUtilities.Entities;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Infrastructure.DataAccess;
using PolicyDesk.Infrastructure.DataAccess.Repositories;
using PolicyDesk.Infrastructure.Tests.Policyholders;
using Shouldly;

namespace PolicyDesk.Infrastructure.Tests.Policies;

public class PoliciesRepositoryTests(PostgreSqlInfrastructureFixture fixture) : PoliciesRepositoryTestBase(fixture)
{
    [Fact(DisplayName = nameof(Deve_AdicionarEBuscarApolice_Quando_ApoliceForValida))]
    [Trait("Infrastructure", "PoliciesRepository - Cadastro e Consulta")]
    public async Task Deve_AdicionarEBuscarApolice_Quando_ApoliceForValida()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var repository = CreateRepository(dbContext);
        var policyholder = PolicyholderBuilder.Build();
        dbContext.Policyholders.Add(policyholder);
        await dbContext.SaveChangesAsync();
        var policy = PolicyBuilder.Build(policyholder.Id);

        await repository.AddAsync(policy);
        await new UnitOfWork(dbContext).CommitAsync();

        var saved = await repository.GetByIdAsync(policy.Id);
        saved.ShouldNotBeNull();
        saved.Id.ShouldBe(policy.Id);
        saved.PolicyholderId.ShouldBe(policyholder.Id);
        saved.Description.ShouldBe(policy.Description);
        saved.CoverageAmount.ShouldBe(policy.CoverageAmount);
        saved.PremiumAmount.ShouldBe(policy.PremiumAmount);
        saved.Status.ShouldBe(PolicyStatus.Active);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNulo_Quando_BuscarApoliceInexistente))]
    [Trait("Infrastructure", "PoliciesRepository - Consulta")]
    public async Task Deve_RetornarNulo_Quando_BuscarApoliceInexistente()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var repository = CreateRepository(dbContext);

        (await repository.GetByIdAsync(Guid.NewGuid())).ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_AplicarFiltrosDeSeguradoEStatus_Quando_Listar))]
    [Trait("Infrastructure", "PoliciesRepository - Consulta")]
    public async Task Deve_AplicarFiltrosDeSeguradoEStatus_Quando_Listar()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var repository = CreateRepository(dbContext);
        var firstHolder = PolicyholderBuilder.Build();
        var secondHolder = PolicyholderBuilder.Build();
        dbContext.Policyholders.AddRange(firstHolder, secondHolder);
        await dbContext.SaveChangesAsync();

        var activeForFirst = PolicyBuilder.Build(firstHolder.Id);
        var cancelledForFirst = PolicyBuilder.Build(firstHolder.Id);
        cancelledForFirst.Cancel();
        var activeForSecond = PolicyBuilder.Build(secondHolder.Id);
        dbContext.Policies.AddRange(activeForFirst, cancelledForFirst, activeForSecond);
        await dbContext.SaveChangesAsync();

        var byHolder = await repository.GetAllAsync(firstHolder.Id, null);
        var byStatus = await repository.GetAllAsync(firstHolder.Id, PolicyStatus.Cancelled);
        var byBoth = await repository.GetAllAsync(firstHolder.Id, PolicyStatus.Active);

        byHolder.Select(policy => policy.Id).ShouldBe([activeForFirst.Id, cancelledForFirst.Id], ignoreOrder: true);
        byStatus.Select(policy => policy.Id).ShouldBe([cancelledForFirst.Id]);
        byBoth.Select(policy => policy.Id).ShouldBe([activeForFirst.Id]);
    }

    [Fact(DisplayName = nameof(Deve_PersistirCancelamento_Quando_ConfirmarUnidadeDeTrabalho))]
    [Trait("Infrastructure", "PoliciesRepository - Persistência")]
    public async Task Deve_PersistirCancelamento_Quando_ConfirmarUnidadeDeTrabalho()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var policyholder = PolicyholderBuilder.Build();
        dbContext.Policyholders.Add(policyholder);
        await dbContext.SaveChangesAsync();
        var policy = PolicyBuilder.Build(policyholder.Id);
        dbContext.Policies.Add(policy);
        await dbContext.SaveChangesAsync();

        var repository = CreateRepository(dbContext);
        var trackedPolicy = await repository.GetByIdForUpdateAsync(policy.Id);
        trackedPolicy.ShouldNotBeNull();
        trackedPolicy.Cancel();
        await new UnitOfWork(dbContext).CommitAsync();

        await using var verificationContext = Fixture.CreateDbContext();
        var saved = await verificationContext.Policies.AsNoTracking().SingleAsync(item => item.Id == policy.Id);
        saved.Status.ShouldBe(PolicyStatus.Cancelled);
        saved.CancelledAt.ShouldNotBeNull();
        saved.UpdatedAt.ShouldBe(saved.CancelledAt);
    }


    [Fact(DisplayName = nameof(Deve_RastrearSomente_Quando_BuscarApoliceParaAlteracao))]
    [Trait("Infrastructure", "PoliciesRepository - Rastreamento")]
    public async Task Deve_RastrearSomente_Quando_BuscarApoliceParaAlteracao()
    {
        await using var setupContext = Fixture.CreateDbContext();
        var policyholder = PolicyholderBuilder.Build();
        setupContext.Policyholders.Add(policyholder);
        var policy = PolicyBuilder.Build(policyholder.Id);
        setupContext.Policies.Add(policy);
        await setupContext.SaveChangesAsync();

        await using var readContext = Fixture.CreateDbContext();
        (await CreateRepository(readContext).GetByIdAsync(policy.Id)).ShouldNotBeNull();
        readContext.ChangeTracker.Entries<Policy>().ShouldBeEmpty();

        await using var updateContext = Fixture.CreateDbContext();
        (await CreateRepository(updateContext).GetByIdForUpdateAsync(policy.Id)).ShouldNotBeNull();
        updateContext.ChangeTracker.Entries<Policy>().ShouldHaveSingleItem();
    }
}
