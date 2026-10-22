namespace PolicyDesk.Application.Policyholders.Deactivate;

public interface IDeactivatePolicyholderUseCase
{
    Task Execute(Guid policyholderId);
}