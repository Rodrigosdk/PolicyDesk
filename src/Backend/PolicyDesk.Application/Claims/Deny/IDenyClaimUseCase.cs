using PolicyDesk.Communication.Requests;

namespace PolicyDesk.Application.Claims.Deny;

public interface IDenyClaimUseCase
{
    Task ExecuteAsync(Guid claimId, RequestDenyClaimJson request, CancellationToken cancellationToken = default);
}
