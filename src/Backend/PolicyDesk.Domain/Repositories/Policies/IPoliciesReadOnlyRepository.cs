using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;

namespace PolicyDesk.Domain.Repositories.Policies;

public interface IPoliciesReadOnlyRepository
{
    Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Policy>> GetAllAsync(Guid? policyholderId, PolicyStatus? status, CancellationToken cancellationToken = default);
}
