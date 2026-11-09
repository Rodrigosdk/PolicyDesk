using PolicyDesk.Application.Common;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Repositories.Claims;
using PolicyDesk.Domain.Repositories.Policies;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Claims.Listing;

internal class ListClaimsUseCase(
    IClaimsReadOnlyRepository claimsReadOnlyRepository,
    IPoliciesReadOnlyRepository policiesReadOnlyRepository) : IListClaimsUseCase
{
    public async Task<IEnumerable<ResponseClaimJson>> ListByPolicyAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        if (await policiesReadOnlyRepository.GetByIdAsync(policyId, cancellationToken) is null)
            throw new NotFoundException(ResourcesMessageException.POLICY_NOT_FOUND);

        var claims = await claimsReadOnlyRepository.GetByPolicyIdAsync(policyId, cancellationToken);
        return claims.Select(MapToResponse);
    }

    public async Task<ResponseClaimJson> GetByIdAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        var claim = await claimsReadOnlyRepository.GetByIdAsync(claimId, cancellationToken)
                    ?? throw new NotFoundException(ResourcesMessageException.CLAIM_NOT_FOUND);
        return MapToResponse(claim);
    }

    private static ResponseClaimJson MapToResponse(Domain.Entities.Claim claim) => new()
    {
        Id = claim.Id,
        ApoliceId = claim.PolicyId,
        Descricao = claim.Description,
        Valor = claim.Amount,
        DataOcorrencia = claim.OccurredOn,
        Status = claim.Status.ToContract(),
        MotivoNegacao = claim.DenialReason,
        DataAprovacao = claim.ApprovedAt,
        DataPagamento = claim.PaidAt,
        CriadoEm = claim.CreatedAt,
        AtualizadoEm = claim.UpdatedAt
    };
}
