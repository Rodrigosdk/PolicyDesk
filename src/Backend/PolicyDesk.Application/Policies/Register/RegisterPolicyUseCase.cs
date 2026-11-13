using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Policies;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Policies.Register;

internal class RegisterPolicyUseCase(
    IPoliciesWriteOnlyRepository policiesWriteOnlyRepository,
    IPolicyholdersUpdateOnlyRepository policyholdersUpdateOnlyRepository,
    IUnitOfWork unitOfWork,
    ILogger<RegisterPolicyUseCase> logger) : IRegisterPolicyUseCase
{
    public async Task<ResponseCreatedJson> ExecuteAsync(RequestPolicyJson request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var policyholder = await policyholdersUpdateOnlyRepository.GetByIdIncludingInactiveAsync(request.SeguradoId, cancellationToken)
                           ?? throw new NotFoundException(ResourcesMessageException.POLICYHOLDER_NOT_FOUND);

        if (!policyholder.Active)
        {
            throw new ErrorOnValidationException(ResourcesMessageException.POLICYHOLDER_INACTIVE);
        }

        var policy = Policy.Create(
            request.SeguradoId,
            request.Descricao,
            request.ValorCobertura,
            request.ValorPremio,
            request.InicioVigencia,
            request.FimVigencia);

        policyholder.RegisterPolicy();

        await policiesWriteOnlyRepository.AddAsync(policy);
        await unitOfWork.CommitAsync(cancellationToken);
        logger.PolicyRegistered(policy.Id, policyholder.Id);

        return new ResponseCreatedJson { Id = policy.Id };
    }

    private static void Validate(RequestPolicyJson request)
    {
        var errors = new List<string>();

        if (request.SeguradoId == Guid.Empty)
            errors.Add(ResourcesMessageException.POLICYHOLDER_REQUIRED);

        if (string.IsNullOrWhiteSpace(request.Descricao))
            errors.Add(ResourcesMessageException.DESCRIPTION_REQUIRED);
        else if (FieldLimits.Exceeds(request.Descricao, FieldLimits.Description))
            errors.Add(FieldLimits.DescriptionTooLong);

        if (request.ValorCobertura <= 0)
            errors.Add(ResourcesMessageException.COVERAGE_AMOUNT_MUST_BE_POSITIVE);
        else if (!MoneyLimits.IsValid(request.ValorCobertura))
            errors.Add(MoneyLimits.CoverageAmountInvalidPrecision);

        if (request.ValorPremio <= 0)
            errors.Add(ResourcesMessageException.PREMIUM_AMOUNT_MUST_BE_POSITIVE);
        else if (!MoneyLimits.IsValid(request.ValorPremio))
            errors.Add(MoneyLimits.PremiumAmountInvalidPrecision);

        if (request.InicioVigencia == default || request.FimVigencia == default)
            errors.Add(ResourcesMessageException.POLICY_DATES_REQUIRED);
        else if (request.FimVigencia <= request.InicioVigencia)
            errors.Add(ResourcesMessageException.POLICY_END_DATE_MUST_BE_AFTER_START_DATE);

        if (errors.Count > 0)
            throw new ErrorOnValidationException(errors);
    }
}
