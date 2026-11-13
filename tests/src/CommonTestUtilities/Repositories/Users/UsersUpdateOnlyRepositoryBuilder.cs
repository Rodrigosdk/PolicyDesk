using Moq;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories.Users;

namespace CommonTestUtilities.Repositories.Users;

public class UsersUpdateOnlyRepositoryBuilder
{
    private readonly Mock<IUsersUpdateOnlyRepository> _mock = new();

    public void GetByIdForUpdate(User user)
    {
        _mock.Setup(repository => repository.GetByIdForUpdateAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
    }

    public void GetByIdIncludingInactive(User user)
    {
        _mock.Setup(repository => repository.GetByIdIncludingInactiveAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
    }

    public void GetLatestInactiveByEmailForUpdate(User user)
    {
        _mock
            .Setup(repository => repository.GetLatestInactiveByEmailForUpdateAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    public IUsersUpdateOnlyRepository Build() => _mock.Object;
}
