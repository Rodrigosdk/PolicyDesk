using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;

namespace PolicyDesk.Application.Policyholders.Register;

public interface IRegisterPolicyholderUseCase
{
    public Task<ResponseCreatedJson> ExecuteAsync(RequestPolicyholderJson request, CancellationToken cancellationToken = default);
}