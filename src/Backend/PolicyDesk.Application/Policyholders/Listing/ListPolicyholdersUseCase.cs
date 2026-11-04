using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policyholders.Listing;

internal class ListPolicyholdersUseCase(
    IPolicyholdersReadOnlyRepository policyholdersReadOnlyRepository) : IListPolicyholdersUseCase
{
    public async Task<IEnumerable<ResponsePolicyholdersListingJson>> Listing(CancellationToken cancellationToken = default)
    {
        var policyholders = await policyholdersReadOnlyRepository.GetAll(cancellationToken);
        return policyholders.Select(MapToResponse);
    }

    public async Task<ResponsePolicyholdersListingJson> GetById(Guid policyholderId, CancellationToken cancellationToken = default)
    {
        var policyholder = await policyholdersReadOnlyRepository.GetById(policyholderId, cancellationToken)
                           ?? throw new NotFoundException(ResourcesMessageException.POLICYHOLDER_NOT_FOUND);
        return MapToResponse(policyholder);
    }

    private static ResponsePolicyholdersListingJson MapToResponse(Domain.Entities.Policyholders policyholder) => new()
    {
        Id = policyholder.Id,
        Name = policyholder.Name,
        Email = policyholder.Email,
        Cpf = policyholder.Cpf,
        BirthDate = policyholder.BirthDate,
        Phone = policyholder.Phone,
        CreatedAt = policyholder.CreatedAt,
        UpdatedAt = policyholder.UpdatedAt
    };
}
