using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Application.Policyholders.Listing;

public interface IListPolicyholdersUseCase
{
    Task<IEnumerable<ResponsePolicyholdersListingJson>> ListAsync(CancellationToken cancellationToken = default);
    Task<ResponsePolicyholdersListingJson> GetByIdAsync(Guid policyholderId, CancellationToken cancellationToken = default);
}