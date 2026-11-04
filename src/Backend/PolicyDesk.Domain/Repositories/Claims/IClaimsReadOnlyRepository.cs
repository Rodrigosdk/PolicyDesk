using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Domain.Repositories.Claims;

public interface IClaimsReadOnlyRepository
{
    Task<Claim?> GetById(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Claim>> GetByPolicyId(Guid policyId, CancellationToken cancellationToken = default);
}
