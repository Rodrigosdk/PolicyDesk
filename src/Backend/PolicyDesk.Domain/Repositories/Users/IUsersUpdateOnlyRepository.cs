using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Domain.Repositories.Users;

public interface IUsersUpdateOnlyRepository
{
    Task<User?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByIdIncludingInactiveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetLatestInactiveByEmailForUpdateAsync(string email, CancellationToken cancellationToken = default);
}
