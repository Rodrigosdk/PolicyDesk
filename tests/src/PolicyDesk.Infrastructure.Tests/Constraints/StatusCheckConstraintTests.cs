using CommonTestUtilities.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Infrastructure.Tests.Policyholders;
using Shouldly;

namespace PolicyDesk.Infrastructure.Tests.Constraints;

public class StatusCheckConstraintTests(PostgreSqlInfrastructureFixture fixture) : IClassFixture<PostgreSqlInfrastructureFixture>
{
    [Fact(DisplayName = nameof(Deve_GravarStatusComoTexto_Quando_SalvarApoliceESinistro))]
    [Trait("Infrastructure", "Constraints - Status")]
    public async Task Deve_GravarStatusComoTexto_Quando_SalvarApoliceESinistro()
    {
        await using var dbContext = fixture.CreateDbContext();
        var (policyId, claimId) = await SavePolicyWithClaim(dbContext);

        var policyStatus = await dbContext.Database
            .SqlQuery<string>($"SELECT status AS \"Value\" FROM policies WHERE id = {policyId}")
            .SingleAsync();
        var claimStatus = await dbContext.Database
            .SqlQuery<string>($"SELECT status AS \"Value\" FROM claims WHERE id = {claimId}")
            .SingleAsync();

        policyStatus.ShouldBe(nameof(PolicyStatus.Active));
        claimStatus.ShouldBe(nameof(ClaimStatus.InAnalysis));
    }

    [Theory(DisplayName = nameof(Deve_RecusarStatusForaDoEnum_Quando_GravarDiretamenteNoBanco))]
    [Trait("Infrastructure", "Constraints - Status")]
    [InlineData("policies", "CK_policies_status")]
    [InlineData("claims", "CK_claims_status")]
    public async Task Deve_RecusarStatusForaDoEnum_Quando_GravarDiretamenteNoBanco(string table, string constraint)
    {
        await using var dbContext = fixture.CreateDbContext();
        var (policyId, claimId) = await SavePolicyWithClaim(dbContext);

        var exception = await Should.ThrowAsync<PostgresException>(() => table == "policies"
            ? dbContext.Database.ExecuteSqlAsync($"UPDATE policies SET status = 'Unknown' WHERE id = {policyId}")
            : dbContext.Database.ExecuteSqlAsync($"UPDATE claims SET status = 'Unknown' WHERE id = {claimId}"));

        exception.SqlState.ShouldBe(PostgresErrorCodes.CheckViolation);
        exception.ConstraintName.ShouldBe(constraint);
    }

    private static async Task<(Guid PolicyId, Guid ClaimId)> SavePolicyWithClaim(DataAccess.PolicyDeskDbContext dbContext)
    {
        var policyholder = PolicyholderBuilder.Build();
        var policy = PolicyBuilder.Build(policyholder.Id);
        var claim = ClaimBuilder.BuildWithStatus(ClaimStatus.InAnalysis, policy.Id);
        dbContext.Policyholders.Add(policyholder);
        dbContext.Policies.Add(policy);
        dbContext.Claims.Add(claim);
        await dbContext.SaveChangesAsync();
        return (policy.Id, claim.Id);
    }
}
