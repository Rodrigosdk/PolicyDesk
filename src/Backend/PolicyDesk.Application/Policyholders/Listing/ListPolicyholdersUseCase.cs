using PolicyDesk.Application.Common;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policyholders.Listing;

internal class ListPolicyholdersUseCase(
    IPolicyholdersReadOnlyRepository policyholdersReadOnlyRepository) : IListPolicyholdersUseCase
{
    public async Task<ResponsePagedJson<ResponsePolicyholdersListingJson>> ListAsync(RequestPaginationJson pagination, CancellationToken cancellationToken = default)
    {
        var errors = Pagination.Validate(pagination);
        if (errors.Count > 0)
            throw new ErrorOnValidationException(errors);

        var page = await policyholdersReadOnlyRepository.GetPageAsync(pagination.Pagina, pagination.TamanhoPagina, cancellationToken);
        return Pagination.ToResponse(page, pagination, MapToResponse);
    }

    public async Task<ResponsePolicyholdersListingJson> GetByIdAsync(Guid policyholderId, CancellationToken cancellationToken = default)
    {
        var policyholder = await policyholdersReadOnlyRepository.GetByIdAsync(policyholderId, cancellationToken)
                           ?? throw new NotFoundException(ResourcesMessageException.POLICYHOLDER_NOT_FOUND);
        return MapToResponse(policyholder);
    }

    private static ResponsePolicyholdersListingJson MapToResponse(Domain.Entities.Policyholder policyholder) => new()
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
