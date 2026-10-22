namespace PolicyDesk.Domain.Repositories.Policyholders;

public interface IPolicyholdersWriteOnlyRepository
{
    Task<PolicyholderRegistrationResult> RegisterOrReactivate(Entities.Policyholders policyholder);
}