using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Policies;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policies.Cancel;

internal class CancelPolicyUseCase(
    IPoliciesUpdateOnlyRepository policiesUpdateOnlyRepository,
    IUnitOfWork unitOfWork,
    ILogger<CancelPolicyUseCase> logger) : ICancelPolicyUseCase
{
    public async Task ExecuteAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        var policy = await policiesUpdateOnlyRepository.GetByIdForUpdateAsync(policyId, cancellationToken)
                     ?? throw new NotFoundException(ResourcesMessageException.POLICY_NOT_FOUND);

        policy.Cancel();
        await unitOfWork.CommitAsync(cancellationToken);
        logger.PolicyCancelled(policy.Id);
    }
}
