using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Domain.Repositories.Claims;

public interface IClaimsUpdateOnlyRepository
{
    Task<Claim?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
}
