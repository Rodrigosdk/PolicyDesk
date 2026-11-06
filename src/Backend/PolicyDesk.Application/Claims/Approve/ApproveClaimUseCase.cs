using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Claims;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Claims.Approve;

internal class ApproveClaimUseCase(
    IClaimsUpdateOnlyRepository claimsUpdateOnlyRepository,
    IUnitOfWork unitOfWork,
    ILogger<ApproveClaimUseCase> logger) : IApproveClaimUseCase
{
    public async Task ExecuteAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        var claim = await claimsUpdateOnlyRepository.GetByIdForUpdateAsync(claimId, cancellationToken)
                    ?? throw new NotFoundException(ResourcesMessageException.CLAIM_NOT_FOUND);

        claim.Approve();
        await unitOfWork.CommitAsync(cancellationToken);
        logger.ClaimApproved(claim.Id);
    }
}
