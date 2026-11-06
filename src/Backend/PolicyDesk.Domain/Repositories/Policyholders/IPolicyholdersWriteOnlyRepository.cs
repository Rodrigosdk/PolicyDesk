namespace PolicyDesk.Domain.Repositories.Policyholders;

public interface IPolicyholdersWriteOnlyRepository
{
    Task<PolicyholderRegistrationResult> RegisterOrReactivateAsync(Entities.Policyholder policyholder, CancellationToken cancellationToken = default);
}