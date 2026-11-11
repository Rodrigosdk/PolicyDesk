using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Application.Policies.Register;

public interface IRegisterPolicyUseCase
{
    Task<ResponseCreatedJson> ExecuteAsync(RequestPolicyJson request, CancellationToken cancellationToken = default);
}
