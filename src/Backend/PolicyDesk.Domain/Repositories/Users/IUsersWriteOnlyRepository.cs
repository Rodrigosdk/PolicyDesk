using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Domain.Repositories.Users;

public interface IUsersWriteOnlyRepository
{
    Task AddAsync(User user);
}
