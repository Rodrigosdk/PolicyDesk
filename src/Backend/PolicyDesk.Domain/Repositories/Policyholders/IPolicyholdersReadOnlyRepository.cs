namespace PolicyDesk.Domain.Repositories.Policyholders;

public interface IPolicyholdersReadOnlyRepository
{
    Task<IEnumerable<Entities.Policyholders>> GetAll(CancellationToken cancellationToken = default);
    Task<Entities.Policyholders?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistActivePolicyholdersWithEmail(string email, Guid? exceptPolicyholderId = null, CancellationToken cancellationToken = default);
    // Apólice não cancelada que ainda não terminou: vigente hoje ou com vigência futura
    Task<bool> HasActiveOrFuturePolicy(Guid policyholderId, CancellationToken cancellationToken = default);
}