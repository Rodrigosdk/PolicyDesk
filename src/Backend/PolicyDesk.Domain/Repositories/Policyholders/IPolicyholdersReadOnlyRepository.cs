namespace PolicyDesk.Domain.Repositories.Policyholders;

public interface IPolicyholdersReadOnlyRepository
{
    Task<PagedResult<Entities.Policyholder>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Entities.Policyholder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistActivePolicyholdersWithEmailAsync(string email, Guid? exceptPolicyholderId = null, CancellationToken cancellationToken = default);
    // Apólice não cancelada que ainda não terminou: vigente hoje ou com vigência futura
    Task<bool> HasActiveOrFuturePolicyAsync(Guid policyholderId, DateOnly today, CancellationToken cancellationToken = default);
}