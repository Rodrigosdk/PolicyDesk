using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Enums;

namespace PolicyDesk.Application.Policies.Listing;

public interface IListPoliciesUseCase
{
    Task<IEnumerable<ResponsePolicyJson>> List(Guid? policyholderId, PolicyStatus? status, CancellationToken cancellationToken = default);
    Task<ResponsePolicyJson> GetById(Guid policyId, CancellationToken cancellationToken = default);
}
