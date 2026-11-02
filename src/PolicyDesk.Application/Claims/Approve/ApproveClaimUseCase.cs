using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Claims;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Claims.Approve;

internal class ApproveClaimUseCase(
    IClaimsReadOnlyRepository claimsReadOnlyRepository,
    IUnitOfWork unitOfWork) : IApproveClaimUseCase
{
    public async Task Execute(Guid claimId)
    {
        var claim = await claimsReadOnlyRepository.GetById(claimId)
                    ?? throw new NotFoundException(ResourcesMessageException.CLAIM_NOT_FOUND);

        if (claim.Status != ClaimStatus.InAnalysis)
        {
            throw new ErrorOnValidationException(ResourcesMessageException.CLAIM_MUST_BE_IN_ANALYSIS_FOR_APPROVAL);
        }

        claim.Approve();

        await unitOfWork.Commit();
    }
}
