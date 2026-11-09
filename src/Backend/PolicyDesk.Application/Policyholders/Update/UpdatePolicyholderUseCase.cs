using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policyholders.Update;

internal class UpdatePolicyholderUseCase(
    IPolicyholdersReadOnlyRepository policyholdersReadOnlyRepository,
    IPolicyholdersUpdateOnlyRepository policyholdersUpdateOnlyRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdatePolicyholderUseCase> logger)
    : IUpdatePolicyholderUseCase
{
    public async Task ExecuteAsync(Guid policyholderId, RequestUpdatePolicyholderJson request, CancellationToken cancellationToken = default)
    {
        var policyholder = await ValidateAndThrowOnFailuresAsync(policyholderId, request, cancellationToken);
        
        policyholder.Update(
            request.Nome,
            request.DataNascimento,
            request.Email,
            request.Telefone
        );
        
        await unitOfWork.CommitAsync(cancellationToken);
        logger.PolicyholderUpdated(policyholder.Id);
    }
    private async Task<Domain.Entities.Policyholder> ValidateAndThrowOnFailuresAsync(
        Guid policyholderId, RequestUpdatePolicyholderJson request, CancellationToken cancellationToken)
    {
        var errors = PolicyholderDataValidator.Validate(
            request.Nome,
            request.DataNascimento,
            request.Email,
            request.Telefone);

        if (errors.Count > 0)
        {
            throw new ErrorOnValidationException(errors);
        }
        var policyholder = await policyholdersUpdateOnlyRepository.GetByIdForUpdateAsync(policyholderId, cancellationToken);
        
        if (policyholder == null)
        {
            throw new NotFoundException(ResourcesMessageException.POLICYHOLDER_NOT_FOUND);
        }
        
        var normalizedEmail = EmailValidator.Normalize(request.Email);
        var existActivePolicyholdersWithEmail = await policyholdersReadOnlyRepository.ExistActivePolicyholdersWithEmailAsync(normalizedEmail, policyholderId, cancellationToken);
        
        return existActivePolicyholdersWithEmail
            ? throw new ErrorOnValidationException(ResourcesMessageException.POLICYHOLDER_ACTIVE_EMAIL_EXISTS)
            : policyholder;
    }
}