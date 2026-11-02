using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Policies;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policies.Cancel;

internal class CancelPolicyUseCase(
    IPoliciesReadOnlyRepository policiesReadOnlyRepository,
    IUnitOfWork unitOfWork) : ICancelPolicyUseCase
{
    public async Task Execute(Guid policyId)
    {
        var policy = await policiesReadOnlyRepository.GetById(policyId)
                     ?? throw new NotFoundException(ResourcesMessageException.POLICY_NOT_FOUND);

        policy.Cancel();
        await unitOfWork.Commit();
    }
}
