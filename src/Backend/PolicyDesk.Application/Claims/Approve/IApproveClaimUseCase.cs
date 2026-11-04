namespace PolicyDesk.Application.Claims.Approve;

public interface IApproveClaimUseCase
{
    Task Execute(Guid claimId, CancellationToken cancellationToken = default);
}
