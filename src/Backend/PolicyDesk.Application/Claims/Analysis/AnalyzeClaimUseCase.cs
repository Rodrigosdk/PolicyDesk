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
    public async Task ExecuteAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        var claim = await claimsUpdateOnlyRepository.GetByIdForUpdateAsync(claimId, cancellationToken)
                    ?? throw new NotFoundException(ResourcesMessageException.CLAIM_NOT_FOUND);

        claim.StartAnalysis();
        await unitOfWork.CommitAsync(cancellationToken);
        logger.ClaimAnalysisStarted(claim.Id);
    }
}
