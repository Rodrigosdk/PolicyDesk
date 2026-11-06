namespace PolicyDesk.Application.Claims.Approve;

public interface IApproveClaimUseCase
{
    Task ExecuteAsync(Guid claimId, CancellationToken cancellationToken = default);
}
