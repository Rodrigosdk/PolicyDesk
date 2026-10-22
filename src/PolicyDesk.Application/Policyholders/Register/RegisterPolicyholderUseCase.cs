using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policyholders.Register;

internal class RegisterPolicyholderUseCase(
    IPolicyholdersWriteOnlyRepository policyholdersWriteOnlyRepository) : IRegisterPolicyholderUseCase
{
    
    public async Task Execute(RequestPolicyholdersJson request)
    {
        ValidateAndThrowOnFailures(request);

        var policyholder = new Domain.Entities.Policyholders(
            request.Name,
            request.Cpf,
            request.BirthDate,
            request.Email,
            request.PhoneNumber
        );

        var result = await policyholdersWriteOnlyRepository.RegisterOrReactivate(policyholder);
        switch (result)
        {
            case PolicyholderRegistrationResult.ActiveCpfAlreadyExists:
                throw new ErrorOnValidationException(ResourcesMessageException.POLICYHOLDER_ACTIVE_CPF_EXISTS);
            case PolicyholderRegistrationResult.ActiveEmailAlreadyExists:
                throw new ErrorOnValidationException(ResourcesMessageException.POLICYHOLDER_ACTIVE_EMAIL_EXISTS);
            case PolicyholderRegistrationResult.RegisteredOrReactivated:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private static void ValidateAndThrowOnFailures(RequestPolicyholdersJson request)
    {
        var validatorCpf = CpfValidator.IsValid(request.Cpf);
        var validatorEmail = EmailValidator.IsValid(request.Email);
        
        if (!validatorCpf)
        {
            throw new ErrorOnValidationException(ResourcesMessageException.INVALID_CPF);
        }

        if (!validatorEmail)
        {
            throw new ErrorOnValidationException(ResourcesMessageException.INVALID_EMAIL);
        }
    }
    
}