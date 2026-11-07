using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Application.Policyholders.Listing;

public interface IListPolicyholdersUseCase
{
    Task<ResponsePagedJson<ResponsePolicyholdersListingJson>> ListAsync(RequestPaginationJson pagination, CancellationToken cancellationToken = default);
    Task<ResponsePolicyholdersListingJson> GetByIdAsync(Guid policyholderId, CancellationToken cancellationToken = default);
}