using Microsoft.EntityFrameworkCore;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Repositories.Claims;

namespace PolicyDesk.Infrastructure.DataAccess.Repositories;

internal sealed class ClaimsRepository(PolicyDeskDbContext dbContext) :
    IClaimsWriteOnlyRepository,
    IClaimsReadOnlyRepository,
    IClaimsUpdateOnlyRepository
{
    public Task AddAsync(Claim claim)
    {
        dbContext.Claims.Add(claim);
        return Task.CompletedTask;
    }

    public Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Claims.AsNoTracking().FirstOrDefaultAsync(claim => claim.Id == id, cancellationToken);
    }

    public Task<Claim?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Claims.FirstOrDefaultAsync(claim => claim.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Claim>> GetByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Claims
            .AsNoTracking()
            .Where(claim => claim.PolicyId == policyId)
            .OrderBy(claim => claim.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<decimal> SumCommittedAmountAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        return dbContext.Claims
            .Where(claim => claim.PolicyId == policyId && claim.Status != ClaimStatus.Denied)
            .SumAsync(claim => claim.Amount, cancellationToken);
    }
}
