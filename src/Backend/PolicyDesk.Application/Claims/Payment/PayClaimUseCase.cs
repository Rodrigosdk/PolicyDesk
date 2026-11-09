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
    public async Task ExecuteAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        var claim = await claimsUpdateOnlyRepository.GetByIdForUpdateAsync(claimId, cancellationToken)
                    ?? throw new NotFoundException(ResourcesMessageException.CLAIM_NOT_FOUND);

        claim.RegisterPayment();
        await unitOfWork.CommitAsync(cancellationToken);
        logger.ClaimPaid(claim.Id, claim.Amount);
    }
}
