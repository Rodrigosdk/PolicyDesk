using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policyholders.Update;

internal class UpdatePolicyholderCommand(
    IPolicyholdersReadOnlyRepository policyholdersReadOnlyRepository,
    IPolicyholdersUpdateOnlyRepository policyholdersUpdateOnlyRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdatePolicyholderCommand> logger)
    : IUpdatePolicyholderCommand
{
    public async Task Execute(Guid policyholderId, RequestPolicyholdersUpdateJson request, CancellationToken cancellationToken = default)
    {
        var policyholder = await ValidateAndThrowOnFailures(policyholderId, request, cancellationToken);
        
        policyholder.Update(
            request.Name,
            request.BirthDate,
            request.Email,
            request.PhoneNumber
        );
        
        await unitOfWork.Commit(cancellationToken);
        logger.PolicyholderUpdated(policyholder.Id);
    }
    private async Task<Domain.Entities.Policyholders> ValidateAndThrowOnFailures(
        Guid policyholderId, RequestPolicyholdersUpdateJson request, CancellationToken cancellationToken)
    {
        var errors = PolicyholderDataValidator.Validate(
            request.Name,
            request.BirthDate,
            request.Email,
            request.PhoneNumber);

        if (errors.Count > 0)
        {
            throw new ErrorOnValidationException(errors);
        }
        var policyholder = await policyholdersUpdateOnlyRepository.GetByIdForUpdate(policyholderId, cancellationToken);
        
        if (policyholder == null)
        {
            throw new NotFoundException(ResourcesMessageException.POLICYHOLDER_NOT_FOUND);
        }
        
        var normalizedEmail = EmailValidator.Normalize(request.Email);
        var existActivePolicyholdersWithEmail = await policyholdersReadOnlyRepository.ExistActivePolicyholdersWithEmail(normalizedEmail, policyholderId, cancellationToken);
        
        return existActivePolicyholdersWithEmail
            ? throw new ErrorOnValidationException(ResourcesMessageException.POLICYHOLDER_ACTIVE_EMAIL_EXISTS)
            : policyholder;
    }
}