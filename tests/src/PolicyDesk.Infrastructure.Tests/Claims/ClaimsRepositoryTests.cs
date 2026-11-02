using CommonTestUtilities.Entities;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Infrastructure.DataAccess;
using PolicyDesk.Infrastructure.Tests.Policyholders;
using Shouldly;

namespace PolicyDesk.Infrastructure.Tests.Claims;

public class ClaimsRepositoryTests(PostgreSqlInfrastructureFixture fixture) : ClaimsRepositoryTestBase(fixture)
{
    private static async Task<Policy> SavePolicy(PolicyDeskDbContext dbContext)
    {
        var policyholder = PolicyholderBuilder.Build();
        dbContext.Policyholders.Add(policyholder);
        var policy = PolicyBuilder.Build(policyholder.Id);
        dbContext.Policies.Add(policy);
        await dbContext.SaveChangesAsync();
        return policy;
    }

    [Fact(DisplayName = nameof(Deve_AdicionarEBuscarSinistro_Quando_SinistroForValido))]
    [Trait("Infrastructure", "ClaimsRepository - Cadastro e Consulta")]
    public async Task Deve_AdicionarEBuscarSinistro_Quando_SinistroForValido()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var repository = CreateRepository(dbContext);
        var policy = await SavePolicy(dbContext);
        var claim = ClaimBuilder.Build(policy.Id, amount: 750.55m);

        await repository.Add(claim);
        await new UnitOfWork(dbContext).Commit();

        await using var verificationContext = Fixture.CreateDbContext();
        var saved = await CreateRepository(verificationContext).GetById(claim.Id);
        saved.ShouldNotBeNull();
        saved.PolicyId.ShouldBe(policy.Id);
        saved.Description.ShouldBe(claim.Description);
        saved.Amount.ShouldBe(750.55m);
        saved.OccurredOn.ShouldBe(claim.OccurredOn);
        saved.Status.ShouldBe(ClaimStatus.Pending);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNulo_Quando_BuscarSinistroInexistente))]
    [Trait("Infrastructure", "ClaimsRepository - Consulta")]
    public async Task Deve_RetornarNulo_Quando_BuscarSinistroInexistente()
    {
        await using var dbContext = Fixture.CreateDbContext();

        (await CreateRepository(dbContext).GetById(Guid.NewGuid())).ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_RetornarApenasSinistrosDaApolice_Quando_BuscarPorApolice))]
    [Trait("Infrastructure", "ClaimsRepository - Consulta")]
    public async Task Deve_RetornarApenasSinistrosDaApolice_Quando_BuscarPorApolice()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var repository = CreateRepository(dbContext);
        var policy = await SavePolicy(dbContext);
        var otherPolicy = await SavePolicy(dbContext);
        var first = ClaimBuilder.Build(policy.Id);
        await Task.Delay(10);
        var second = ClaimBuilder.Build(policy.Id);
        var other = ClaimBuilder.Build(otherPolicy.Id);
        dbContext.Claims.AddRange(second, other, first);
        await dbContext.SaveChangesAsync();

        var result = await repository.GetByPolicyId(policy.Id);

        result.Select(claim => claim.Id).ShouldBe([first.Id, second.Id]);
        (await repository.GetByPolicyId(Guid.NewGuid())).ShouldBeEmpty();
    }

    [Fact(DisplayName = nameof(Deve_PersistirFluxoDoSinistro_Quando_ConfirmarUnidadeDeTrabalho))]
    [Trait("Infrastructure", "ClaimsRepository - Persistência")]
    public async Task Deve_PersistirFluxoDoSinistro_Quando_ConfirmarUnidadeDeTrabalho()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var policy = await SavePolicy(dbContext);
        var claim = ClaimBuilder.Build(policy.Id);
        dbContext.Claims.Add(claim);
        await dbContext.SaveChangesAsync();

        var tracked = await CreateRepository(dbContext).GetById(claim.Id);
        tracked.ShouldNotBeNull();
        tracked.StartAnalysis();
        tracked.Approve();
        tracked.RegisterPayment();
        await new UnitOfWork(dbContext).Commit();

        await using var verificationContext = Fixture.CreateDbContext();
        var saved = await verificationContext.Claims.AsNoTracking().SingleAsync(item => item.Id == claim.Id);
        saved.Status.ShouldBe(ClaimStatus.Paid);
        saved.ApprovedAt.ShouldNotBeNull();
        saved.PaidAt.ShouldNotBeNull();
        saved.UpdatedAt.ShouldBe(saved.PaidAt);
    }

    [Fact(DisplayName = nameof(Deve_PersistirMotivoDeNegacao_Quando_SinistroForNegado))]
    [Trait("Infrastructure", "ClaimsRepository - Persistência")]
    public async Task Deve_PersistirMotivoDeNegacao_Quando_SinistroForNegado()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var policy = await SavePolicy(dbContext);
        var claim = ClaimBuilder.Build(policy.Id);
        claim.StartAnalysis();
        claim.Deny("Not covered");
        dbContext.Claims.Add(claim);
        await dbContext.SaveChangesAsync();

        await using var verificationContext = Fixture.CreateDbContext();
        var saved = await verificationContext.Claims.AsNoTracking().SingleAsync(item => item.Id == claim.Id);
        saved.Status.ShouldBe(ClaimStatus.Denied);
        saved.DenialReason.ShouldBe("Not covered");
    }

    [Fact(DisplayName = nameof(Deve_RejeitarSinistro_Quando_ApoliceNaoExistir))]
    [Trait("Infrastructure", "ClaimsRepository - Integridade")]
    public async Task Deve_RejeitarSinistro_Quando_ApoliceNaoExistir()
    {
        await using var dbContext = Fixture.CreateDbContext();
        dbContext.Claims.Add(ClaimBuilder.Build());

        await Should.ThrowAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }
}
