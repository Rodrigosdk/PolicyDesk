using PolicyDesk.Application.Common;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Repositories.Policies;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policies.Listing;

internal class ListPoliciesUseCase(IPoliciesReadOnlyRepository policiesReadOnlyRepository) : IListPoliciesUseCase
{
    public async Task<ResponsePagedJson<ResponsePolicyJson>> ListAsync(
        Guid? policyholderId,
        PolicyStatus? status,
        RequestPaginationJson pagination,
        CancellationToken cancellationToken = default)
    {
        var errors = Pagination.Validate(pagination);
        if (status.HasValue && !Enum.IsDefined(status.Value))
            errors.Insert(0, ResourcesMessageException.INVALID_POLICY_STATUS);

        if (errors.Count > 0)
            throw new ErrorOnValidationException(errors);

        var page = await policiesReadOnlyRepository.GetPageAsync(
            policyholderId,
            status,
            pagination.Pagina,
            pagination.TamanhoPagina,
            cancellationToken);
        return Pagination.ToResponse(page, pagination, MapToResponse);
    }

    public async Task<ResponsePolicyJson> GetByIdAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        var policy = await policiesReadOnlyRepository.GetByIdAsync(policyId, cancellationToken)
                     ?? throw new NotFoundException(ResourcesMessageException.POLICY_NOT_FOUND);
        return MapToResponse(policy);
    }

    private static ResponsePolicyJson MapToResponse(Domain.Entities.Policy policy) => new()
    {
        Id = policy.Id,
        SeguradoId = policy.PolicyholderId,
        Descricao = policy.Description,
        ValorCobertura = policy.CoverageAmount,
        ValorPremio = policy.PremiumAmount,
        InicioVigencia = policy.StartDate,
        FimVigencia = policy.EndDate,
        Status = policy.Status,
        CreatedAt = policy.CreatedAt,
        UpdatedAt = policy.UpdatedAt
    };
}
