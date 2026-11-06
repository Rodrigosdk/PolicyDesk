using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Application.Claims.Listing;

public interface IListClaimsUseCase
{
    Task<IEnumerable<ResponseClaimJson>> ListByPolicyAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<ResponseClaimJson> GetByIdAsync(Guid claimId, CancellationToken cancellationToken = default);
}
