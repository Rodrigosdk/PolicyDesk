namespace PolicyDesk.Application.Policies.Cancel;

public interface ICancelPolicyUseCase
{
    Task ExecuteAsync(Guid policyId, CancellationToken cancellationToken = default);
}
