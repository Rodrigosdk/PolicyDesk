using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Application.Policyholders.Listing;

public interface IListPolicyholdersUseCase
{
    Task<IEnumerable<ResponsePolicyholdersListingJson>> Listing(CancellationToken cancellationToken = default);
    Task<ResponsePolicyholdersListingJson> GetById(Guid policyholderId, CancellationToken cancellationToken = default);
}