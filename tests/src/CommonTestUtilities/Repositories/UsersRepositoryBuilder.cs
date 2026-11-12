using Moq;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories.Users;

namespace CommonTestUtilities.Repositories;

public class UsersReadOnlyRepositoryBuilder
{
    private readonly Mock<IUsersReadOnlyRepository> _repository = new();

    public Mock<IUsersReadOnlyRepository> Mock => _repository;

    public UsersReadOnlyRepositoryBuilder GetById(User user)
    {
        _repository.Setup(repository => repository.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        return this;
    }

    public UsersReadOnlyRepositoryBuilder GetByEmail(User user)
    {
        _repository.Setup(repository => repository.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        return this;
    }

    public UsersReadOnlyRepositoryBuilder ExistActiveWithEmail()
    {
        _repository
            .Setup(repository => repository.ExistActiveUserWithEmailAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        return this;
    }

    public IUsersReadOnlyRepository Build() => _repository.Object;
}

public class UsersUpdateOnlyRepositoryBuilder
{
    private readonly Mock<IUsersUpdateOnlyRepository> _repository = new();

    public Mock<IUsersUpdateOnlyRepository> Mock => _repository;

    public UsersUpdateOnlyRepositoryBuilder GetByIdForUpdate(User user)
    {
        _repository.Setup(repository => repository.GetByIdForUpdateAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        return this;
    }

    public UsersUpdateOnlyRepositoryBuilder GetByIdIncludingInactive(User user)
    {
        _repository.Setup(repository => repository.GetByIdIncludingInactiveAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        return this;
    }

    public IUsersUpdateOnlyRepository Build() => _repository.Object;
}

public static class UsersWriteOnlyRepositoryBuilder
{
    public static Mock<IUsersWriteOnlyRepository> Build()
    {
        var mock = new Mock<IUsersWriteOnlyRepository>();
        mock.Setup(repository => repository.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        return mock;
    }
}
