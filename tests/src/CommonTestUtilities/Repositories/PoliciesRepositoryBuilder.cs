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
        _repository.Setup(repository => repository.GetById(policy.Id)).ReturnsAsync(policy);
        return this;
    }

    public PoliciesReadOnlyRepositoryBuilder GetAll(IEnumerable<Policy> policies)
    {
        _repository
            .Setup(repository => repository.GetAll(It.IsAny<Guid?>(), It.IsAny<PolicyStatus?>()))
            .ReturnsAsync(policies);
        return this;
    }

    public Mock<IPoliciesReadOnlyRepository> Mock => _repository;

    public IPoliciesReadOnlyRepository Build() => _repository.Object;
}

public static class PoliciesWriteOnlyRepositoryBuilder
{
    public static Mock<IPoliciesWriteOnlyRepository> Build()
    {
        var mock = new Mock<IPoliciesWriteOnlyRepository>();
        mock.Setup(repository => repository.Add(It.IsAny<Policy>())).Returns(Task.CompletedTask);
        return mock;
    }
}
