using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Claims;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Claims.Payment;

internal class PayClaimUseCase(
    IClaimsUpdateOnlyRepository claimsUpdateOnlyRepository,
    IUnitOfWork unitOfWork,
    ILogger<PayClaimUseCase> logger) : IPayClaimUseCase
{
    public async Task Execute(Guid claimId)
    {
        var claim = await claimsUpdateOnlyRepository.GetByIdForUpdate(claimId)
                    ?? throw new NotFoundException(ResourcesMessageException.CLAIM_NOT_FOUND);

        claim.RegisterPayment();
        await unitOfWork.Commit();
        logger.ClaimPaid(claim.Id, claim.Amount);
    }
}
