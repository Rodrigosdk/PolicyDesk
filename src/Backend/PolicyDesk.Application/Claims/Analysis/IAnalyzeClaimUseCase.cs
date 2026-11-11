namespace PolicyDesk.Application.Claims.Analysis;

public interface IAnalyzeClaimUseCase
{
    Task ExecuteAsync(Guid claimId, CancellationToken cancellationToken = default);
}
