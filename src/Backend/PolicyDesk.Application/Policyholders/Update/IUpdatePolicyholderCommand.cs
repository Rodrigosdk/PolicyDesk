using PolicyDesk.Communication.Requests;

namespace PolicyDesk.Application.Policyholders.Update;

public interface IUpdatePolicyholderCommand
{
    Task Execute(Guid policyholderId, RequestPolicyholdersUpdateJson request);
}