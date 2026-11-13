using Moq;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Policies;

namespace CommonTestUtilities.Repositories.Policies;

public class PoliciesReadOnlyRepositoryBuilder
{
    private readonly Mock<IPoliciesReadOnlyRepository> _mock = new();

    public void GetById(Policy policy)
    {
        _mock.Setup(repository => repository.GetByIdAsync(policy.Id, It.IsAny<CancellationToken>())).ReturnsAsync(policy);
    }

    public void GetPage(IEnumerable<Policy> policies, int? totalCount = null)
    {
        var items = policies.ToList();
        _mock
            .Setup(repository => repository.GetPageAsync(It.IsAny<Guid?>(), It.IsAny<PolicyStatus?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Policy>(items, totalCount ?? items.Count));
    }

    public IPoliciesReadOnlyRepository Build() => _mock.Object;
}
