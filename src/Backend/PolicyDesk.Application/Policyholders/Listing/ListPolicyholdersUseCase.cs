using PolicyDesk.Application.Common;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policyholders.Listing;

internal class ListPolicyholdersUseCase(
    IPolicyholdersReadOnlyRepository policyholdersReadOnlyRepository) : IListPolicyholdersUseCase
{
    public async Task<ResponsePagedJson<ResponsePolicyholderJson>> ListAsync(RequestPaginationJson pagination, CancellationToken cancellationToken = default)
    {
        var errors = Pagination.Validate(pagination);
        if (errors.Count > 0)
            throw new ErrorOnValidationException(errors);

        var page = await policyholdersReadOnlyRepository.GetPageAsync(pagination.Pagina, pagination.TamanhoPagina, cancellationToken);
        return Pagination.ToResponse(page, pagination, MapToResponse);
    }

    public async Task<ResponsePolicyholderJson> GetByIdAsync(Guid policyholderId, CancellationToken cancellationToken = default)
    {
        var policyholder = await policyholdersReadOnlyRepository.GetByIdAsync(policyholderId, cancellationToken)
                           ?? throw new NotFoundException(ResourcesMessageException.POLICYHOLDER_NOT_FOUND);
        return MapToResponse(policyholder);
    }

    private static ResponsePolicyholderJson MapToResponse(Policyholder policyholder) => new()
    {
        Id = policyholder.Id,
        Nome = policyholder.Name,
        Cpf = policyholder.Cpf,
        DataNascimento = policyholder.BirthDate,
        Email = policyholder.Email,
        Telefone = policyholder.Phone,
        CriadoEm = policyholder.CreatedAt,
        AtualizadoEm = policyholder.UpdatedAt
    };
}
