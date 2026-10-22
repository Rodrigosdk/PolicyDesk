using Moq;
using PolicyDesk.Domain.Repositories.Policyholders;
using Policyholder = PolicyDesk.Domain.Entities.Policyholders;

namespace CommonTestUtilities.Repositories;

public class PolicyholdersReadOnlyRepositoryBuilder
{
    private readonly Mock<IPolicyholdersReadOnlyRepository> _repository = new();

    public Mock<IPolicyholdersReadOnlyRepository> Mock => _repository;

    public PolicyholdersReadOnlyRepositoryBuilder GetAll(IEnumerable<Policyholder> policyholders)
    {
        _repository.Setup(r => r.GetAll()).ReturnsAsync(policyholders);
        return this;
    }

    public PolicyholdersReadOnlyRepositoryBuilder GetById(Policyholder policyholder)
    {
        _repository.Setup(r => r.GetById(policyholder.Id)).ReturnsAsync(policyholder);
        return this;
    }

    public PolicyholdersReadOnlyRepositoryBuilder ExistActiveWithEmail()
    {
        _repository
            .Setup(r => r.ExistActivePolicyholdersWithEmail(It.IsAny<string>(), It.IsAny<Guid?>()))
            .ReturnsAsync(true);
        return this;
    }

    public PolicyholdersReadOnlyRepositoryBuilder HasActivePolicy(Guid policyholderId, bool value = true)
    {
        _repository.Setup(r => r.HasActivePolicy(policyholderId)).ReturnsAsync(value);
        return this;
    }

    public IPolicyholdersReadOnlyRepository Build() => _repository.Object;
}

public static class PolicyholdersWriteOnlyRepositoryBuilder
{
    public static Mock<IPolicyholdersWriteOnlyRepository> Build(
        PolicyholderRegistrationResult result = PolicyholderRegistrationResult.RegisteredOrReactivated)
    {
        var mock = new Mock<IPolicyholdersWriteOnlyRepository>();
        mock.Setup(r => r.RegisterOrReactivate(It.IsAny<Policyholder>())).ReturnsAsync(result);
        return mock;
    }
}
