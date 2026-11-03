using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Claims;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Claims.Approve;

internal class ApproveClaimUseCase(
    IClaimsReadOnlyRepository claimsReadOnlyRepository,
    IUnitOfWork unitOfWork,
    ILogger<ApproveClaimUseCase> logger) : IApproveClaimUseCase
{
    public async Task Execute(Guid claimId)
    {
        var claim = await claimsReadOnlyRepository.GetById(claimId)
                    ?? throw new NotFoundException(ResourcesMessageException.CLAIM_NOT_FOUND);

        claim.Approve();
        await unitOfWork.Commit();
        logger.ClaimApproved(claim.Id);
    }
}
