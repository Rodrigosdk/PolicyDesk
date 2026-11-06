using Moq;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Repositories.Policies;

namespace CommonTestUtilities.Repositories;

public class PoliciesReadOnlyRepositoryBuilder
{
    private readonly Mock<IPoliciesReadOnlyRepository> _repository = new();

    public PoliciesReadOnlyRepositoryBuilder GetById(Policy policy)
    {
        _repository.Setup(repository => repository.GetByIdAsync(policy.Id, It.IsAny<CancellationToken>())).ReturnsAsync(policy);
        return this;
    }

    public PoliciesReadOnlyRepositoryBuilder GetAll(IEnumerable<Policy> policies)
    {
        _repository
            .Setup(repository => repository.GetAllAsync(It.IsAny<Guid?>(), It.IsAny<PolicyStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(policies);
        return this;
    }

    public Mock<IPoliciesReadOnlyRepository> Mock => _repository;

    public IPoliciesReadOnlyRepository Build() => _repository.Object;
}

public class PoliciesUpdateOnlyRepositoryBuilder
{
    private readonly Mock<IPoliciesUpdateOnlyRepository> _repository = new();

    public Mock<IPoliciesUpdateOnlyRepository> Mock => _repository;

    public PoliciesUpdateOnlyRepositoryBuilder GetByIdForUpdate(Policy policy)
    {
        _repository.Setup(repository => repository.GetByIdForUpdateAsync(policy.Id, It.IsAny<CancellationToken>())).ReturnsAsync(policy);
        return this;
    }

    public IPoliciesUpdateOnlyRepository Build() => _repository.Object;
}

public static class PoliciesWriteOnlyRepositoryBuilder
{
    public static Mock<IPoliciesWriteOnlyRepository> Build()
    {
        var mock = new Mock<IPoliciesWriteOnlyRepository>();
        mock.Setup(repository => repository.AddAsync(It.IsAny<Policy>())).Returns(Task.CompletedTask);
        return mock;
    }
}
