using Moq;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories.Policies;

namespace CommonTestUtilities.Repositories.Policies;

public class PoliciesUpdateOnlyRepositoryBuilder
{
    private readonly Mock<IPoliciesUpdateOnlyRepository> _mock = new();

    public void GetByIdForUpdate(Policy policy)
    {
        _mock.Setup(repository => repository.GetByIdForUpdateAsync(policy.Id, It.IsAny<CancellationToken>())).ReturnsAsync(policy);
    }

    public IPoliciesUpdateOnlyRepository Build() => _mock.Object;
}
