using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Application.Claims.Register;

public interface IRegisterClaimUseCase
{
    Task<ResponseCreatedJson> Execute(Guid policyId, RequestClaimJson request);
}
