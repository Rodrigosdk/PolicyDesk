using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policyholders.Deactivate;

internal class DeactivatePolicyholderUseCase(
    IPolicyholdersReadOnlyRepository policyholdersReadOnlyRepository,
    IUnitOfWork unitOfWork) : IDeactivatePolicyholderUseCase
{
    public async Task Execute(Guid policyholderId)
    {
        var policyholder = await policyholdersReadOnlyRepository.GetById(policyholderId);
        if (policyholder == null)
        {
            throw new NotFoundException(ResourcesMessageException.POLICYHOLDER_NOT_FOUND);
        }

        if (await policyholdersReadOnlyRepository.HasActivePolicy(policyholderId))
        {
            throw new ErrorOnValidationException(ResourcesMessageException.POLICYHOLDER_HAS_ACTIVE_POLICY);
        }
        
        policyholder.Deactivate();
        
        await unitOfWork.Commit();
    }
}