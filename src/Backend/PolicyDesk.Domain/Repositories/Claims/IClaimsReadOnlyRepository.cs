using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Domain.Repositories.Claims;

public interface IClaimsReadOnlyRepository
{
    Task<Claim?> GetById(Guid id);
    Task<IEnumerable<Claim>> GetByPolicyId(Guid policyId);
}
