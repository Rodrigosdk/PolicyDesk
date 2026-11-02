using CommonTestUtilities.Entities;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
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

    [Fact(DisplayName = nameof(Deve_LancarConflitoSemSobrescrever_Quando_SinistroForAlteradoPorOutraOperacao))]
    [Trait("Infrastructure", "UnitOfWork - Concorrência")]
    public async Task Deve_LancarConflitoSemSobrescrever_Quando_SinistroForAlteradoPorOutraOperacao()
    {
        var claimId = await SaveClaimInAnalysis();

        // Duas "requisições" leem o mesmo sinistro em InAnalysis antes de qualquer uma gravar
        await using var firstRequest = Fixture.CreateDbContext();
        await using var secondRequest = Fixture.CreateDbContext();
        var claimInFirst = await firstRequest.Claims.SingleAsync(claim => claim.Id == claimId);
        var claimInSecond = await secondRequest.Claims.SingleAsync(claim => claim.Id == claimId);

        claimInFirst.Approve();
        await new UnitOfWork(firstRequest).Commit();

        claimInSecond.Deny("Not covered");
        var exception = await Should.ThrowAsync<ConflictException>(() => new UnitOfWork(secondRequest).Commit());

        exception.Message.ShouldBe(ResourcesMessageException.CONCURRENCY_CONFLICT);
        await using var verificationContext = Fixture.CreateDbContext();
        var saved = await verificationContext.Claims.AsNoTracking().SingleAsync(claim => claim.Id == claimId);
        saved.Status.ShouldBe(ClaimStatus.Approved);
        saved.ApprovedAt.ShouldNotBeNull();
        saved.DenialReason.ShouldBeNull();
    }

    [Fact(DisplayName = nameof(Deve_LancarConflito_Quando_PagamentoForRegistradoDuasVezesAoMesmoTempo))]
    [Trait("Infrastructure", "UnitOfWork - Concorrência")]
    public async Task Deve_LancarConflito_Quando_PagamentoForRegistradoDuasVezesAoMesmoTempo()
    {
        var claimId = await SaveClaimInAnalysis();
        await using (var approval = Fixture.CreateDbContext())
        {
            (await approval.Claims.SingleAsync(claim => claim.Id == claimId)).Approve();
            await new UnitOfWork(approval).Commit();
        }

        await using var firstRequest = Fixture.CreateDbContext();
        await using var secondRequest = Fixture.CreateDbContext();
        var claimInFirst = await firstRequest.Claims.SingleAsync(claim => claim.Id == claimId);
        var claimInSecond = await secondRequest.Claims.SingleAsync(claim => claim.Id == claimId);

        claimInFirst.RegisterPayment();
        await new UnitOfWork(firstRequest).Commit();

        claimInSecond.RegisterPayment();
        await Should.ThrowAsync<ConflictException>(() => new UnitOfWork(secondRequest).Commit());
    }

    [Fact(DisplayName = nameof(Deve_PermitirGravacoesSeguidas_Quando_UsarOMesmoContexto))]
    [Trait("Infrastructure", "UnitOfWork - Concorrência")]
    public async Task Deve_PermitirGravacoesSeguidas_Quando_UsarOMesmoContexto()
    {
        var claimId = await SaveClaimInAnalysis();
        await using var dbContext = Fixture.CreateDbContext();
        var unitOfWork = new UnitOfWork(dbContext);
        var claim = await dbContext.Claims.SingleAsync(item => item.Id == claimId);

        // Depois de cada gravação o EF lê o xmin novo; a gravação seguinte não pode ser vista como conflito
        claim.Approve();
        await unitOfWork.Commit();
        claim.RegisterPayment();
        await unitOfWork.Commit();

        await using var verificationContext = Fixture.CreateDbContext();
        (await verificationContext.Claims.AsNoTracking().SingleAsync(item => item.Id == claimId))
            .Status.ShouldBe(ClaimStatus.Paid);
    }

    [Fact(DisplayName = nameof(Deve_LancarValidacao_Quando_DoisSeguradosRecebemOMesmoEmailAoMesmoTempo))]
    [Trait("Infrastructure", "UnitOfWork - Concorrência")]
    public async Task Deve_LancarValidacao_Quando_DoisSeguradosRecebemOMesmoEmailAoMesmoTempo()
    {
        await using var setupContext = await Fixture.CreateCleanDbContextAsync();
        var repository = CreateRepository(setupContext);
        var first = PolicyholderBuilder.Build();
        var second = PolicyholderBuilder.Build();
        await repository.RegisterOrReactivate(first);
        await repository.RegisterOrReactivate(second);
        const string sharedEmail = "shared.email@example.com";

        // Cada "requisição" já verificou que o e-mail estava livre antes de gravar
        await using var firstRequest = Fixture.CreateDbContext();
        await using var secondRequest = Fixture.CreateDbContext();
        await UpdateEmail(firstRequest, first.Id, sharedEmail);
        await UpdateEmail(secondRequest, second.Id, sharedEmail);

        await new UnitOfWork(firstRequest).Commit();
        var exception = await Should.ThrowAsync<ErrorOnValidationException>(() => new UnitOfWork(secondRequest).Commit());

        exception.GetErrors().ShouldBe([ResourcesMessageException.POLICYHOLDER_ACTIVE_EMAIL_EXISTS]);
        await using var verificationContext = Fixture.CreateDbContext();
        var savedSecond = await verificationContext.Policyholders.AsNoTracking().SingleAsync(p => p.Id == second.Id);
        savedSecond.Email.ShouldBe(second.Email);
    }

    private async Task<Guid> SaveClaimInAnalysis()
    {
        await using var dbContext = Fixture.CreateDbContext();
        var policyholder = PolicyholderBuilder.Build();
        dbContext.Policyholders.Add(policyholder);
        var policy = PolicyBuilder.Build(policyholder.Id);
        dbContext.Policies.Add(policy);
        var claim = ClaimBuilder.BuildWithStatus(ClaimStatus.InAnalysis, policy.Id);
        dbContext.Claims.Add(claim);
        await dbContext.SaveChangesAsync();
        return claim.Id;
    }

    private static async Task UpdateEmail(PolicyDeskDbContext dbContext, Guid policyholderId, string email)
    {
        var policyholder = await dbContext.Policyholders.SingleAsync(p => p.Id == policyholderId);
        policyholder.Update(policyholder.Name, policyholder.BirthDate, email, policyholder.Phone);
    }
}
