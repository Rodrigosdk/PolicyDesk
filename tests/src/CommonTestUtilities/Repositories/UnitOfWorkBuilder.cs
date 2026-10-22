using Moq;
using PolicyDesk.Domain.Repositories;

namespace CommonTestUtilities.Repositories;

public static class UnitOfWorkBuilder
{
    public static Mock<IUnitOfWork> Build()
    {
        var mock = new Mock<IUnitOfWork>();
        mock.Setup(u => u.Commit()).Returns(Task.CompletedTask);
        return mock;
    }
}
