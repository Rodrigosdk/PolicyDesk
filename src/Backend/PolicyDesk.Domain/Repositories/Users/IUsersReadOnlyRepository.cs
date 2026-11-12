using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Domain.Repositories.Users;

public interface IUsersReadOnlyRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistActiveUserWithEmailAsync(string email, Guid? exceptUserId = null, CancellationToken cancellationToken = default);
}
