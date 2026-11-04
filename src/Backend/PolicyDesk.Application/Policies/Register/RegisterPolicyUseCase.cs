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
    public async Task<ResponseCreatedJson> Execute(RequestPolicyJson request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var policyholder = await policyholdersUpdateOnlyRepository.GetByIdIncludingInactive(request.PolicyholderId, cancellationToken)
                           ?? throw new NotFoundException(ResourcesMessageException.POLICYHOLDER_NOT_FOUND);

        if (!policyholder.Active)
        {
            throw new ErrorOnValidationException(ResourcesMessageException.POLICYHOLDER_INACTIVE);
        }

        var policy = new Policy(
            request.PolicyholderId,
            request.Description,
            request.CoverageAmount,
            request.PremiumAmount,
            request.StartDate,
            request.EndDate);

        policyholder.RegisterPolicy();

        await policiesWriteOnlyRepository.Add(policy);
        await unitOfWork.Commit(cancellationToken);
        logger.PolicyRegistered(policy.Id, policyholder.Id);

        return new ResponseCreatedJson { Id = policy.Id };
    }

    private static void Validate(RequestPolicyJson request)
    {
        var errors = new List<string>();

        if (request.PolicyholderId == Guid.Empty)
            errors.Add(ResourcesMessageException.POLICYHOLDER_REQUIRED);

        if (string.IsNullOrWhiteSpace(request.Description))
            errors.Add(ResourcesMessageException.DESCRIPTION_REQUIRED);
        else if (FieldLimits.Exceeds(request.Description, FieldLimits.Description))
            errors.Add(FieldLimits.DescriptionTooLong);

        if (request.CoverageAmount <= 0)
            errors.Add(ResourcesMessageException.COVERAGE_AMOUNT_MUST_BE_POSITIVE);

        if (request.PremiumAmount <= 0)
            errors.Add(ResourcesMessageException.PREMIUM_AMOUNT_MUST_BE_POSITIVE);

        if (request.StartDate == default || request.EndDate == default)
            errors.Add(ResourcesMessageException.POLICY_DATES_REQUIRED);
        else if (request.EndDate.Date <= request.StartDate.Date)
            errors.Add(ResourcesMessageException.POLICY_END_DATE_MUST_BE_AFTER_START_DATE);

        if (errors.Count > 0)
            throw new ErrorOnValidationException(errors);
    }
}
