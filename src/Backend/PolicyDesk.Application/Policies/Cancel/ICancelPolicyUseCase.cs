namespace PolicyDesk.Application.Policies.Cancel;

public interface ICancelPolicyUseCase
{
    Task Execute(Guid policyId, CancellationToken cancellationToken = default);
}
