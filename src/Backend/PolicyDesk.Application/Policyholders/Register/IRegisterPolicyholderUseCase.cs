using PolicyDesk.Communication.Requests;

namespace PolicyDesk.Application.Policyholders.Register;

public interface IRegisterPolicyholderUseCase
{
    public Task ExecuteAsync(RequestPolicyholdersJson request, CancellationToken cancellationToken = default);
}