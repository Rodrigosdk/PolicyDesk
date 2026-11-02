using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Application.Claims.Listing;

public interface IListClaimsUseCase
{
    Task<IEnumerable<ResponseClaimJson>> ListByPolicy(Guid policyId);
    Task<ResponseClaimJson> GetById(Guid claimId);
}
