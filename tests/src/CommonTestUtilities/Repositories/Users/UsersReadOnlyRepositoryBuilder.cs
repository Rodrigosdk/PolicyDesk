using Moq;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories.Users;

namespace CommonTestUtilities.Repositories.Users;

public class UsersReadOnlyRepositoryBuilder
{
    private readonly Mock<IUsersReadOnlyRepository> _mock = new();

    public void GetById(User user)
    {
        _mock.Setup(repository => repository.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
    }

    public void GetByEmail(User user)
    {
        _mock.Setup(repository => repository.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
    }

    public void ExistActiveWithEmail()
    {
        _mock
            .Setup(repository => repository.ExistActiveUserWithEmailAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    public IUsersReadOnlyRepository Build() => _mock.Object;
}
