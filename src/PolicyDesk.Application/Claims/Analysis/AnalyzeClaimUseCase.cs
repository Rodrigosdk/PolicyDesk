using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Claims;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Claims.Analysis;

internal class AnalyzeClaimUseCase(
    IClaimsReadOnlyRepository claimsReadOnlyRepository,
    IUnitOfWork unitOfWork) : IAnalyzeClaimUseCase
{
    public async Task Execute(Guid claimId)
    {
        var claim = await claimsReadOnlyRepository.GetById(claimId)
                    ?? throw new NotFoundException(ResourcesMessageException.CLAIM_NOT_FOUND);
        if (claim.Status != ClaimStatus.Pending)
            throw new ErrorOnValidationException(ResourcesMessageException.CLAIM_MUST_BE_PENDING_FOR_ANALYSIS);

        claim.StartAnalysis();
        await unitOfWork.Commit();
    }
}
