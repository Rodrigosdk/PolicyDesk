namespace PolicyDesk.Domain.Repositories.Policyholders;

public interface IPolicyholdersWriteOnlyRepository
{
    Task<PolicyholderRegistrationResult> RegisterOrReactivate(Entities.Policyholder policyholder, CancellationToken cancellationToken = default);
}