using PolicyDesk.Communication.Requests;

namespace PolicyDesk.Application.Policyholders.Update;

public interface IUpdatePolicyholderUseCase
{
    Task ExecuteAsync(Guid policyholderId, RequestPolicyholdersUpdateJson request, CancellationToken cancellationToken = default);
}