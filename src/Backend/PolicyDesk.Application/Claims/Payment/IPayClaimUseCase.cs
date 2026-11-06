namespace PolicyDesk.Application.Claims.Payment;

public interface IPayClaimUseCase
{
    Task ExecuteAsync(Guid claimId, CancellationToken cancellationToken = default);
}
