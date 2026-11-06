namespace PolicyDesk.Domain.Repositories.Policyholders;

public interface IPolicyholdersUpdateOnlyRepository
{
    Task<Entities.Policyholder?> GetByIdForUpdate(Guid id, CancellationToken cancellationToken = default);
    // Inclui inativos: para quem precisa distinguir segurado inexistente de segurado inativo
    Task<Entities.Policyholder?> GetByIdIncludingInactive(Guid id, CancellationToken cancellationToken = default);
}
