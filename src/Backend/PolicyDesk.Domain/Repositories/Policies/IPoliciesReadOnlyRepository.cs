using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;

namespace PolicyDesk.Domain.Repositories.Policies;

public interface IPoliciesReadOnlyRepository
{
    Task<Policy?> GetById(Guid id);
    Task<IEnumerable<Policy>> GetAll(Guid? policyholderId, PolicyStatus? status);
}
