using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policyholders.Deactivate;

internal class DeactivatePolicyholderUseCase(
    IPolicyholdersReadOnlyRepository policyholdersReadOnlyRepository,
    IPolicyholdersUpdateOnlyRepository policyholdersUpdateOnlyRepository,
    IUnitOfWork unitOfWork,
    BusinessClock clock,
    ILogger<DeactivatePolicyholderUseCase> logger) : IDeactivatePolicyholderUseCase
{
    public async Task ExecuteAsync(Guid policyholderId, CancellationToken cancellationToken = default)
    {
        var policyholder = await policyholdersUpdateOnlyRepository.GetByIdForUpdateAsync(policyholderId, cancellationToken);
        if (policyholder == null)
        {
            throw new NotFoundException(ResourcesMessageException.POLICYHOLDER_NOT_FOUND);
        }

        if (await policyholdersReadOnlyRepository.HasActiveOrFuturePolicyAsync(policyholderId, clock.Today, cancellationToken))
        {
            throw new ErrorOnValidationException(ResourcesMessageException.POLICYHOLDER_HAS_ACTIVE_POLICY);
        }
        
        policyholder.Deactivate();
        
        await unitOfWork.CommitAsync(cancellationToken);
        logger.PolicyholderDeactivated(policyholder.Id);
    }
}