using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Application.Policyholders.Listing;

public interface IListPolicyholdersUseCase
{
    Task<IEnumerable<ResponsePolicyholdersListingJson>> Listing();
    Task<ResponsePolicyholdersListingJson> GetById(Guid policyholderId);
}