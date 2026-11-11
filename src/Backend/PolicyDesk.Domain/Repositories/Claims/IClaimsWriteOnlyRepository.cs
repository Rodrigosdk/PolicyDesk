using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Domain.Repositories.Claims;

public interface IClaimsWriteOnlyRepository
{
    Task AddAsync(Claim claim);
}
