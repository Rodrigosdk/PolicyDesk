using Moq;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories.Users;

namespace CommonTestUtilities.Repositories.Users;

public static class UsersWriteOnlyRepositoryBuilder
{
    public static IUsersWriteOnlyRepository Build()
    {
        var mock = new Mock<IUsersWriteOnlyRepository>();
        mock.Setup(repository => repository.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        return mock.Object;
    }
}

