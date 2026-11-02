using Microsoft.EntityFrameworkCore;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories.Claims;

namespace PolicyDesk.Infrastructure.DataAccess.Repositories;

internal sealed class ClaimsRepository(PolicyDeskDbContext dbContext) :
    IClaimsWriteOnlyRepository,
    IClaimsReadOnlyRepository
{
    public Task Add(Claim claim)
    {
        dbContext.Claims.Add(claim);
        return Task.CompletedTask;
    }

    public Task<Claim?> GetById(Guid id)
    {
        return dbContext.Claims.FirstOrDefaultAsync(claim => claim.Id == id);
    }

    public async Task<IEnumerable<Claim>> GetByPolicyId(Guid policyId)
    {
        return await dbContext.Claims
            .AsNoTracking()
            .Where(claim => claim.PolicyId == policyId)
            .OrderBy(claim => claim.CreatedAt)
            .ToListAsync();
    }
}
