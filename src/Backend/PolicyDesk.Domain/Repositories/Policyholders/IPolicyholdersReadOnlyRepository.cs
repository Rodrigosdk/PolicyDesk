namespace PolicyDesk.Domain.Repositories.Policyholders;

public interface IPolicyholdersReadOnlyRepository
{
    Task<IEnumerable<Entities.Policyholders>> GetAll();
    Task<Entities.Policyholders?> GetById(Guid id);
    // Inclui inativos: para quem precisa distinguir segurado inexistente de segurado inativo
    Task<Entities.Policyholders?> GetByIdIncludingInactive(Guid id);
    Task<bool> ExistActivePolicyholdersWithEmail(string email, Guid? exceptPolicyholderId = null);
    // Apólice não cancelada que ainda não terminou: vigente hoje ou com vigência futura
    Task<bool> HasActiveOrFuturePolicy(Guid policyholderId);
}