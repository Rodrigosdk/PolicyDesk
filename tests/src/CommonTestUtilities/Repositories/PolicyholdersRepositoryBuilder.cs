using Moq;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Domain.Entities;

namespace CommonTestUtilities.Repositories;

public class PolicyholdersReadOnlyRepositoryBuilder
{
    private readonly Mock<IPolicyholdersReadOnlyRepository> _repository = new();

    public Mock<IPolicyholdersReadOnlyRepository> Mock => _repository;

    public PolicyholdersReadOnlyRepositoryBuilder GetPage(IEnumerable<Policyholder> policyholders, int? totalCount = null)
    {
        var items = policyholders.ToList();
        _repository
            .Setup(r => r.GetPageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Policyholder>(items, totalCount ?? items.Count));
        return this;
    }

    public PolicyholdersReadOnlyRepositoryBuilder GetById(Policyholder policyholder)
    {
        _repository.Setup(r => r.GetByIdAsync(policyholder.Id, It.IsAny<CancellationToken>())).ReturnsAsync(policyholder);
        return this;
    }

    public PolicyholdersReadOnlyRepositoryBuilder ExistActiveWithEmail()
    {
        _repository
            .Setup(r => r.ExistActivePolicyholdersWithEmailAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        return this;
    }

    public PolicyholdersReadOnlyRepositoryBuilder HasActiveOrFuturePolicy(Guid policyholderId, bool value = true)
    {
        _repository.Setup(r => r.HasActiveOrFuturePolicyAsync(policyholderId, It.IsAny<CancellationToken>())).ReturnsAsync(value);
        return this;
    }

    public IPolicyholdersReadOnlyRepository Build() => _repository.Object;
}

public class PolicyholdersUpdateOnlyRepositoryBuilder
{
    private readonly Mock<IPolicyholdersUpdateOnlyRepository> _repository = new();

    public Mock<IPolicyholdersUpdateOnlyRepository> Mock => _repository;

    public PolicyholdersUpdateOnlyRepositoryBuilder GetByIdForUpdate(Policyholder policyholder)
    {
        _repository.Setup(r => r.GetByIdForUpdateAsync(policyholder.Id, It.IsAny<CancellationToken>())).ReturnsAsync(policyholder);
        return this;
    }

    public PolicyholdersUpdateOnlyRepositoryBuilder GetByIdIncludingInactive(Policyholder policyholder)
    {
        _repository.Setup(r => r.GetByIdIncludingInactiveAsync(policyholder.Id, It.IsAny<CancellationToken>())).ReturnsAsync(policyholder);
        return this;
    }

    public IPolicyholdersUpdateOnlyRepository Build() => _repository.Object;
}

public static class PolicyholdersWriteOnlyRepositoryBuilder
{
    public static Mock<IPolicyholdersWriteOnlyRepository> Build(
        PolicyholderRegistrationResult result = PolicyholderRegistrationResult.RegisteredOrReactivated)
    {
        var mock = new Mock<IPolicyholdersWriteOnlyRepository>();
        mock.Setup(r => r.RegisterOrReactivateAsync(It.IsAny<Policyholder>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
        return mock;
    }
}
