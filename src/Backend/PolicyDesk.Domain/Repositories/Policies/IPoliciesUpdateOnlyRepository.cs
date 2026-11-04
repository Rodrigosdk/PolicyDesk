using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Domain.Repositories.Policies;

public interface IPoliciesUpdateOnlyRepository
{
    Task<Policy?> GetByIdForUpdate(Guid id);
}
