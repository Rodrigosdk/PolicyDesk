namespace PolicyDesk.Domain.Repositories.Policyholders;

public interface IPolicyholdersUpdateOnlyRepository
{
    Task<Entities.Policyholders?> GetByIdForUpdate(Guid id);
    // Inclui inativos: para quem precisa distinguir segurado inexistente de segurado inativo
    Task<Entities.Policyholders?> GetByIdIncludingInactive(Guid id);
}
