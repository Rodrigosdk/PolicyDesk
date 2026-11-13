using Moq;
using PolicyDesk.Domain.Repositories;

namespace CommonTestUtilities.Repositories;

public static class UnitOfWorkBuilder
{
    public static IUnitOfWork Build()
    {
        var mock = new Mock<IUnitOfWork>();
        mock.Setup(unitOfWork => unitOfWork.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return mock.Object;
    }
}
