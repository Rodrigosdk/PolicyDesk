using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Domain.Repositories.Claims;

public interface IClaimsReadOnlyRepository
{
    Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Claim>> GetByPolicyIdAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<decimal> SumCommittedAmountAsync(Guid policyId, CancellationToken cancellationToken = default);
}
