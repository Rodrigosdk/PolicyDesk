using Microsoft.EntityFrameworkCore;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Repositories.Policies;

namespace PolicyDesk.Infrastructure.DataAccess.Repositories;

internal sealed class PoliciesRepository(PolicyDeskDbContext dbContext) :
    IPoliciesWriteOnlyRepository,
    IPoliciesReadOnlyRepository,
    IPoliciesUpdateOnlyRepository
{
    public Task AddAsync(Policy policy)
    {
        dbContext.Policies.Add(policy);
        return Task.CompletedTask;
    }

    public Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Policies.AsNoTracking().FirstOrDefaultAsync(policy => policy.Id == id, cancellationToken);
    }

    public Task<Policy?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Policies.FirstOrDefaultAsync(policy => policy.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Policy>> GetAllAsync(Guid? policyholderId, PolicyStatus? status, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Policies.AsNoTracking();
        if (policyholderId.HasValue)
            query = query.Where(policy => policy.PolicyholderId == policyholderId.Value);
        if (status.HasValue)
            query = query.Where(policy => policy.Status == status.Value);
        return await query.ToListAsync(cancellationToken);
    }

}
