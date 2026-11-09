namespace PolicyDesk.Domain.Repositories.Policyholders;

public interface IPolicyholdersUpdateOnlyRepository
{
    Task<Entities.Policyholder?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    // Inclui inativos: para quem precisa distinguir segurado inexistente de segurado inativo
    Task<Entities.Policyholder?> GetByIdIncludingInactiveAsync(Guid id, CancellationToken cancellationToken = default);
}
