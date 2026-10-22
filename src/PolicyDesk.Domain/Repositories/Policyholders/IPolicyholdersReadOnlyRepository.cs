namespace PolicyDesk.Domain.Repositories.Policyholders;

public interface IPolicyholdersReadOnlyRepository
{
    Task<IEnumerable<Entities.Policyholders>> GetAll();
    Task<Entities.Policyholders?> GetById(Guid id);
    Task<bool> ExistActivePolicyholdersWithEmail(string email, Guid? exceptPolicyholderId = null);
    Task<bool> HasActivePolicy(Guid policyholderId);
}