using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policyholders.Listing;

public class ListPolicyholdersUseCase(
    IPolicyholdersReadOnlyRepository policyholdersReadOnlyRepository
    ) : IListPolicyholdersUseCase
{
    public Task<IEnumerable<ResponsePolicyholdersListingJson>> Listing()
    {
        var policyholders = policyholdersReadOnlyRepository.GetAll();
        return policyholders.ContinueWith(task =>
        {
            var result = task.Result.Select(MapToResponsePolicyholdersListingJson);
            return result;
        });
    }

    public Task<ResponsePolicyholdersListingJson> GetById(Guid policyholderId)
    {
       return policyholdersReadOnlyRepository.GetById(policyholderId).ContinueWith(task =>
        {
            var p = task.Result;
            return p == null 
                ? throw new NotFoundException(ResourcesMessageException.POLICYHOLDER_NOT_FOUND) 
                : MapToResponsePolicyholdersListingJson(p);
        });
    }
    
    private ResponsePolicyholdersListingJson MapToResponsePolicyholdersListingJson(Domain.Entities.Policyholders p)
    {
        return new ResponsePolicyholdersListingJson
        {
            Id = p.Id,
            Name = p.Name,
            Email = p.Email,
            Cpf = p.Cpf,
            BirthDate = p.BirthDate,
            Phone = p.Phone,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }
}