using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Domain.Repositories.Policies;

public interface IPoliciesWriteOnlyRepository
{
    Task AddAsync(Policy policy);
}
