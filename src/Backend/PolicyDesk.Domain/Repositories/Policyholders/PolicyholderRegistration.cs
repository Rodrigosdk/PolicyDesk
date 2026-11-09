namespace PolicyDesk.Domain.Repositories.Policyholders;

public sealed record PolicyholderRegistration(PolicyholderRegistrationResult Result, Guid? PolicyholderId = null);
