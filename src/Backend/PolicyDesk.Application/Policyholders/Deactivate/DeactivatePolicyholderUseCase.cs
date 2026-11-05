using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policyholders.Deactivate;

internal class DeactivatePolicyholderUseCase(
    IPolicyholdersReadOnlyRepository policyholdersReadOnlyRepository,
    IPolicyholdersUpdateOnlyRepository policyholdersUpdateOnlyRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeactivatePolicyholderUseCase> logger) : IDeactivatePolicyholderUseCase
{
    public async Task Execute(Guid policyholderId, CancellationToken cancellationToken = default)
    {
        var policyholder = await policyholdersUpdateOnlyRepository.GetByIdForUpdate(policyholderId, cancellationToken);
        if (policyholder == null)
        {
            throw new NotFoundException(ResourcesMessageException.POLICYHOLDER_NOT_FOUND);
        }

        if (await policyholdersReadOnlyRepository.HasActiveOrFuturePolicy(policyholderId, cancellationToken))
        {
            throw new ErrorOnValidationException(ResourcesMessageException.POLICYHOLDER_HAS_ACTIVE_POLICY);
        }
        
        policyholder.Deactivate();
        
        await unitOfWork.Commit(cancellationToken);
        logger.PolicyholderDeactivated(policyholder.Id);
    }
}