using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Repositories.Policies;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policies.Listing;

internal class ListPoliciesUseCase(IPoliciesReadOnlyRepository policiesReadOnlyRepository) : IListPoliciesUseCase
{
    public async Task<IEnumerable<ResponsePolicyJson>> List(Guid? policyholderId, PolicyStatus? status, CancellationToken cancellationToken = default)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value))
        {
            throw new ErrorOnValidationException(ResourcesMessageException.INVALID_POLICY_STATUS);
        }

        var policies = await policiesReadOnlyRepository.GetAll(policyholderId, status, cancellationToken);
        return policies.Select(MapToResponse);
    }

    public async Task<ResponsePolicyJson> GetById(Guid policyId, CancellationToken cancellationToken = default)
    {
        var policy = await policiesReadOnlyRepository.GetById(policyId, cancellationToken)
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
