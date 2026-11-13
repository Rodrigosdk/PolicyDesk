using Moq;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Policyholders;

namespace CommonTestUtilities.Repositories.Policyholders;

public class PolicyholdersReadOnlyRepositoryBuilder
{
    private readonly Mock<IPolicyholdersReadOnlyRepository> _mock = new();

    public void GetPage(IEnumerable<Policyholder> policyholders, int? totalCount = null)
    {
        var items = policyholders.ToList();
        _mock
            .Setup(repository => repository.GetPageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Policyholder>(items, totalCount ?? items.Count));
    }

    public void GetPageThrows(System.Exception exception)
    {
        _mock
            .Setup(repository => repository.GetPageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }

    public void GetById(Policyholder policyholder)
    {
        _mock.Setup(repository => repository.GetByIdAsync(policyholder.Id, It.IsAny<CancellationToken>())).ReturnsAsync(policyholder);
    }

    public void ExistActiveWithEmail()
    {
        _mock
            .Setup(repository => repository.ExistActivePolicyholdersWithEmailAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    public void HasActiveOrFuturePolicy(Guid policyholderId, bool value = true, DateOnly? today = null)
    {
        _mock
            .Setup(repository => repository.HasActiveOrFuturePolicyAsync(
                policyholderId,
                It.Is<DateOnly>(date => today == null || date == today),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(value);
    }

    public IPolicyholdersReadOnlyRepository Build() => _mock.Object;
}
