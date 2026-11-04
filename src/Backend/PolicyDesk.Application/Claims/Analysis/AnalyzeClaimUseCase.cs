using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Claims;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Claims.Analysis;

internal class AnalyzeClaimUseCase(
    IClaimsUpdateOnlyRepository claimsUpdateOnlyRepository,
    IUnitOfWork unitOfWork,
    ILogger<AnalyzeClaimUseCase> logger) : IAnalyzeClaimUseCase
{
    public async Task Execute(Guid claimId)
    {
        var claim = await claimsUpdateOnlyRepository.GetByIdForUpdate(claimId)
                    ?? throw new NotFoundException(ResourcesMessageException.CLAIM_NOT_FOUND);

        claim.StartAnalysis();
        await unitOfWork.Commit();
        logger.ClaimAnalysisStarted(claim.Id);
    }
}
