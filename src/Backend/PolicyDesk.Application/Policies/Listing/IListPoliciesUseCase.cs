using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Enums;

namespace PolicyDesk.Application.Policies.Listing;

public interface IListPoliciesUseCase
{
    Task<ResponsePagedJson<ResponsePolicyJson>> ListAsync(Guid? policyholderId, PolicyStatus? status, RequestPaginationJson pagination, CancellationToken cancellationToken = default);
    Task<ResponsePolicyJson> GetByIdAsync(Guid policyId, CancellationToken cancellationToken = default);
}
