namespace PolicyDesk.Application.Claims.Analysis;

public interface IAnalyzeClaimUseCase
{
    Task Execute(Guid claimId, CancellationToken cancellationToken = default);
}
