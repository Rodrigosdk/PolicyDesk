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
    public Task Add(Policy policy)
    {
        dbContext.Policies.Add(policy);
        return Task.CompletedTask;
    }

    public Task<Policy?> GetById(Guid id)
    {
        return dbContext.Policies.AsNoTracking().FirstOrDefaultAsync(policy => policy.Id == id);
    }

    public Task<Policy?> GetByIdForUpdate(Guid id)
    {
        return dbContext.Policies.FirstOrDefaultAsync(policy => policy.Id == id);
    }

    public async Task<IEnumerable<Policy>> GetAll(Guid? policyholderId, PolicyStatus? status)
    {
        var query = dbContext.Policies.AsNoTracking();
        if (policyholderId.HasValue)
            query = query.Where(policy => policy.PolicyholderId == policyholderId.Value);
        if (status.HasValue)
            query = query.Where(policy => policy.Status == status.Value);
        return await query.ToListAsync();
    }

}
