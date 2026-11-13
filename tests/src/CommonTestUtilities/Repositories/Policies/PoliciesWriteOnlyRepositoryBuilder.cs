using Moq;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories.Policies;

namespace CommonTestUtilities.Repositories.Policies;

public static class PoliciesWriteOnlyRepositoryBuilder
{
    public static IPoliciesWriteOnlyRepository Build()
    {
        var mock = new Mock<IPoliciesWriteOnlyRepository>();
        mock.Setup(repository => repository.AddAsync(It.IsAny<Policy>())).Returns(Task.CompletedTask);
        return mock.Object;
    }
}
