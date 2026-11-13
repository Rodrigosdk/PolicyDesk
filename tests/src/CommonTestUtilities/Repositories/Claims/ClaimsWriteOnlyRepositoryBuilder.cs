using Moq;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories.Claims;

namespace CommonTestUtilities.Repositories.Claims;

public static class ClaimsWriteOnlyRepositoryBuilder
{
    public static IClaimsWriteOnlyRepository Build()
    {
        var mock = new Mock<IClaimsWriteOnlyRepository>();
        mock.Setup(repository => repository.AddAsync(It.IsAny<Claim>())).Returns(Task.CompletedTask);
        return mock.Object;
    }
}
