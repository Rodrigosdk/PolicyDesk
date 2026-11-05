namespace PolicyDesk.Application.Claims.Payment;

public interface IPayClaimUseCase
{
    Task Execute(Guid claimId, CancellationToken cancellationToken = default);
}
