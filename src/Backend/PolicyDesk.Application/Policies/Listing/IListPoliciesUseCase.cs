using PolicyDesk.Communication.Enums;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Application.Policies.Listing;

public interface IListPoliciesUseCase
{
    Task<ResponsePagedJson<ResponsePolicyJson>> ListAsync(Guid? policyholderId, StatusApolice? status, RequestPaginationJson pagination, CancellationToken cancellationToken = default);
    Task<ResponsePolicyJson> GetByIdAsync(Guid policyId, CancellationToken cancellationToken = default);
}
