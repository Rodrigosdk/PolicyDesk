namespace PolicyDesk.Domain.Repositories.Policyholders;

public interface IPolicyholdersUpdateOnlyRepository
{
    Task<Entities.Policyholders?> GetByIdForUpdate(Guid id, CancellationToken cancellationToken = default);
    // Inclui inativos: para quem precisa distinguir segurado inexistente de segurado inativo
    Task<Entities.Policyholders?> GetByIdIncludingInactive(Guid id, CancellationToken cancellationToken = default);
}
