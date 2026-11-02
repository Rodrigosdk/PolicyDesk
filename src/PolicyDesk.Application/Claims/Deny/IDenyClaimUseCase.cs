using PolicyDesk.Communication.Requests;

namespace PolicyDesk.Application.Claims.Deny;

public interface IDenyClaimUseCase
{
    Task Execute(Guid claimId, RequestDenyClaimJson request);
}
