using Moq;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories.Policyholders;

namespace CommonTestUtilities.Repositories.Policyholders;

public static class PolicyholdersWriteOnlyRepositoryBuilder
{
    public static IPolicyholdersWriteOnlyRepository Build(
        PolicyholderRegistrationResult result = PolicyholderRegistrationResult.RegisteredOrReactivated)
    {
        var mock = new Mock<IPolicyholdersWriteOnlyRepository>();
        mock
            .Setup(repository => repository.RegisterOrReactivateAsync(It.IsAny<Policyholder>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Policyholder policyholder, CancellationToken _) => new PolicyholderRegistration(
                result,
                result == PolicyholderRegistrationResult.RegisteredOrReactivated ? policyholder.Id : null));
        return mock.Object;
    }
}
