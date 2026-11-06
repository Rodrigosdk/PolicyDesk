using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Domain.Repositories.Policies;

public interface IPoliciesUpdateOnlyRepository
{
    Task<Policy?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
}
