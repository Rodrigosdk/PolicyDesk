using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Claims;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Claims.Payment;

internal class PayClaimUseCase(
    IClaimsReadOnlyRepository claimsReadOnlyRepository,
    IUnitOfWork unitOfWork) : IPayClaimUseCase
{
    public async Task Execute(Guid claimId)
    {
        var claim = await claimsReadOnlyRepository.GetById(claimId)
                    ?? throw new NotFoundException(ResourcesMessageException.CLAIM_NOT_FOUND);
        if (claim.Status != ClaimStatus.Approved)
            throw new ErrorOnValidationException(ResourcesMessageException.CLAIM_MUST_BE_APPROVED_FOR_PAYMENT);

        claim.RegisterPayment();
        await unitOfWork.Commit();
    }
}
