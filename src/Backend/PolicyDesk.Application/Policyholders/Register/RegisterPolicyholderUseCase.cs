using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policyholders.Register;

internal class RegisterPolicyholderUseCase(
    IPolicyholdersWriteOnlyRepository policyholdersWriteOnlyRepository,
    ILogger<RegisterPolicyholderUseCase> logger) : IRegisterPolicyholderUseCase
{
    
    public async Task ExecuteAsync(RequestPolicyholdersJson request, CancellationToken cancellationToken = default)
    {
        ValidateAndThrowOnFailures(request);

        var policyholder = new Domain.Entities.Policyholder(
            request.Name,
            request.Cpf,
            request.BirthDate,
            request.Email,
            request.PhoneNumber
        );

        var result = await policyholdersWriteOnlyRepository.RegisterOrReactivateAsync(policyholder, cancellationToken);
        switch (result)
        {
            case PolicyholderRegistrationResult.ActiveCpfAlreadyExists:
                throw new ErrorOnValidationException(ResourcesMessageException.POLICYHOLDER_ACTIVE_CPF_EXISTS);
            case PolicyholderRegistrationResult.ActiveEmailAlreadyExists:
                throw new ErrorOnValidationException(ResourcesMessageException.POLICYHOLDER_ACTIVE_EMAIL_EXISTS);
            case PolicyholderRegistrationResult.RegisteredOrReactivated:
                logger.PolicyholderRegisteredOrReactivated();
                break;
            default:
                throw new UnreachableException($"Resultado de cadastro inesperado: {result}");
        }
    }
    
    private static void ValidateAndThrowOnFailures(RequestPolicyholdersJson request)
    {
        var errors = PolicyholderDataValidator.Validate(
            request.Name,
            request.BirthDate,
            request.Email,
            request.PhoneNumber);

        if (!CpfValidator.IsValid(request.Cpf))
            errors.Insert(0, ResourcesMessageException.INVALID_CPF);

        if (errors.Count > 0)
            throw new ErrorOnValidationException(errors);
    }
    
}