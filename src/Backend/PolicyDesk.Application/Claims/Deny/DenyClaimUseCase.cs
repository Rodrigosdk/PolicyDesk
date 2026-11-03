using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Claims;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Claims.Deny;

internal class DenyClaimUseCase(
    IClaimsReadOnlyRepository claimsReadOnlyRepository,
    IUnitOfWork unitOfWork) : IDenyClaimUseCase
{
    public async Task Execute(Guid claimId, RequestDenyClaimJson request)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
            throw new ErrorOnValidationException(ResourcesMessageException.CLAIM_DENIAL_REASON_REQUIRED);

        var claim = await claimsReadOnlyRepository.GetById(claimId)
                    ?? throw new NotFoundException(ResourcesMessageException.CLAIM_NOT_FOUND);

        claim.Deny(request.Motivo);
        await unitOfWork.Commit();
    }
}
