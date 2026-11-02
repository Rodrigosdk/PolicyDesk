using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Enums;

namespace PolicyDesk.Application.Policies.Listing;

public interface IListPoliciesUseCase
{
    Task<IEnumerable<ResponsePolicyJson>> List(Guid? policyholderId, PolicyStatus? status);
    Task<ResponsePolicyJson> GetById(Guid policyId);
}
