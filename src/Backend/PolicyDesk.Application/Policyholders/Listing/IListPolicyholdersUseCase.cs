using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Application.Policyholders.Listing;

public interface IListPolicyholdersUseCase
{
    Task<ResponsePagedJson<ResponsePolicyholderJson>> ListAsync(RequestPaginationJson pagination, CancellationToken cancellationToken = default);
    Task<ResponsePolicyholderJson> GetByIdAsync(Guid policyholderId, CancellationToken cancellationToken = default);
}
