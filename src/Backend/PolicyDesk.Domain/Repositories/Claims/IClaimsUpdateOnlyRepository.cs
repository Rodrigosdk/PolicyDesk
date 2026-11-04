using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Domain.Repositories.Claims;

public interface IClaimsUpdateOnlyRepository
{
    Task<Claim?> GetByIdForUpdate(Guid id);
}
