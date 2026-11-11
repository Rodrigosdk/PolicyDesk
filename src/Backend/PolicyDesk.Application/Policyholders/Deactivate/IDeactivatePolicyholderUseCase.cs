namespace PolicyDesk.Application.Policyholders.Deactivate;

public interface IDeactivatePolicyholderUseCase
{
    Task ExecuteAsync(Guid policyholderId, CancellationToken cancellationToken = default);
}