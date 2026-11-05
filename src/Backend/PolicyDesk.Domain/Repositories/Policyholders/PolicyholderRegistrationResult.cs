namespace PolicyDesk.Domain.Repositories.Policyholders;

public enum PolicyholderRegistrationResult
{
    RegisteredOrReactivated,
    ActiveCpfAlreadyExists,
    ActiveEmailAlreadyExists
}
