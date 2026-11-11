using Microsoft.EntityFrameworkCore;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Repositories;
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

    public async Task<PagedResult<Policy>> GetPageAsync(Guid? policyholderId, PolicyStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Policies.AsNoTracking();
        if (policyholderId is { } policyholderIdFilter)
            query = query.Where(policy => policy.PolicyholderId == policyholderIdFilter);
        if (status is { } statusFilter)
            query = query.Where(policy => policy.Status == statusFilter);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(policy => policy.CreatedAt)
            .ThenBy(policy => policy.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Policy>(items, totalCount);
    }

}
