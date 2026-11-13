using Moq;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories.Policyholders;

namespace CommonTestUtilities.Repositories.Policyholders;

public class PolicyholdersUpdateOnlyRepositoryBuilder
{
    private readonly Mock<IPolicyholdersUpdateOnlyRepository> _mock = new();

    public void GetByIdForUpdate(Policyholder policyholder)
    {
        _mock.Setup(repository => repository.GetByIdForUpdateAsync(policyholder.Id, It.IsAny<CancellationToken>())).ReturnsAsync(policyholder);
    }

    public void GetByIdIncludingInactive(Policyholder policyholder)
    {
        _mock.Setup(repository => repository.GetByIdIncludingInactiveAsync(policyholder.Id, It.IsAny<CancellationToken>())).ReturnsAsync(policyholder);
    }

    public IPolicyholdersUpdateOnlyRepository Build() => _mock.Object;
}
