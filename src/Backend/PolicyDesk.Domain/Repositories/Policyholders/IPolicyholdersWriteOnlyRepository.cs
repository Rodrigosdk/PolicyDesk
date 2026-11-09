namespace PolicyDesk.Domain.Repositories.Policyholders;

public interface IPolicyholdersWriteOnlyRepository
{
    Task<PolicyholderRegistration> RegisterOrReactivateAsync(Entities.Policyholder policyholder, CancellationToken cancellationToken = default);
}