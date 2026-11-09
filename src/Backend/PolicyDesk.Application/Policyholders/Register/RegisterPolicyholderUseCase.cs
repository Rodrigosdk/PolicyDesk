using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policyholders.Register;

internal class RegisterPolicyholderUseCase(
    IPolicyholdersWriteOnlyRepository policyholdersWriteOnlyRepository,
    ILogger<RegisterPolicyholderUseCase> logger) : IRegisterPolicyholderUseCase
{
    
    public async Task<ResponseCreatedJson> ExecuteAsync(RequestPolicyholderJson request, CancellationToken cancellationToken = default)
    {
        ValidateAndThrowOnFailures(request);

        var policyholder = new Domain.Entities.Policyholder(
            request.Nome,
            request.Cpf,
            request.DataNascimento,
            request.Email,
            request.Telefone
        );

        var registration = await policyholdersWriteOnlyRepository.RegisterOrReactivateAsync(policyholder, cancellationToken);
        switch (registration.Result)
        {
            case PolicyholderRegistrationResult.ActiveCpfAlreadyExists:
                throw new ErrorOnValidationException(ResourcesMessageException.POLICYHOLDER_ACTIVE_CPF_EXISTS);
            case PolicyholderRegistrationResult.ActiveEmailAlreadyExists:
                throw new ErrorOnValidationException(ResourcesMessageException.POLICYHOLDER_ACTIVE_EMAIL_EXISTS);
            case PolicyholderRegistrationResult.RegisteredOrReactivated:
                logger.PolicyholderRegisteredOrReactivated();
                return new ResponseCreatedJson { Id = registration.PolicyholderId ?? policyholder.Id };
            default:
                throw new UnreachableException($"Resultado de cadastro inesperado: {registration.Result}");
        }
    }
    
    private static void ValidateAndThrowOnFailures(RequestPolicyholderJson request)
    {
        var errors = PolicyholderDataValidator.Validate(
            request.Nome,
            request.DataNascimento,
            request.Email,
            request.Telefone);

        if (!CpfValidator.IsValid(request.Cpf))
            errors.Insert(0, ResourcesMessageException.INVALID_CPF);

        if (errors.Count > 0)
            throw new ErrorOnValidationException(errors);
    }
    
}