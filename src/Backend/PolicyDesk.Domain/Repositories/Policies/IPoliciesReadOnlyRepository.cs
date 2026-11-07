using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;

namespace PolicyDesk.Domain.Repositories.Policies;

public interface IPoliciesReadOnlyRepository
{
    Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Policy>> GetPageAsync(Guid? policyholderId, PolicyStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
}
