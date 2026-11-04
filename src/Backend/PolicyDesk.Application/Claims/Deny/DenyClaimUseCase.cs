using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Claims;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Claims.Deny;

internal class DenyClaimUseCase(
    IClaimsUpdateOnlyRepository claimsUpdateOnlyRepository,
    IUnitOfWork unitOfWork,
    ILogger<DenyClaimUseCase> logger) : IDenyClaimUseCase
{
    public async Task Execute(Guid claimId, RequestDenyClaimJson request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
            throw new ErrorOnValidationException(ResourcesMessageException.CLAIM_DENIAL_REASON_REQUIRED);
        if (FieldLimits.Exceeds(request.Motivo, FieldLimits.DenialReason))
            throw new ErrorOnValidationException(FieldLimits.DenialReasonTooLong);

        var claim = await claimsUpdateOnlyRepository.GetByIdForUpdate(claimId, cancellationToken)
                    ?? throw new NotFoundException(ResourcesMessageException.CLAIM_NOT_FOUND);

        claim.Deny(request.Motivo);
        await unitOfWork.Commit(cancellationToken);
        logger.ClaimDenied(claim.Id);
    }
}
