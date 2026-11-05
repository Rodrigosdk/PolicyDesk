using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Claims;
using PolicyDesk.Domain.Repositories.Policies;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Claims.Register;

internal class RegisterClaimUseCase(
    IClaimsWriteOnlyRepository claimsWriteOnlyRepository,
    IClaimsReadOnlyRepository claimsReadOnlyRepository,
    IPoliciesUpdateOnlyRepository policiesUpdateOnlyRepository,
    IUnitOfWork unitOfWork,
    ILogger<RegisterClaimUseCase> logger) : IRegisterClaimUseCase
{
    public async Task<ResponseCreatedJson> Execute(Guid policyId, RequestClaimJson request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var policy = await policiesUpdateOnlyRepository.GetByIdForUpdate(policyId, cancellationToken)
                     ?? throw new NotFoundException(ResourcesMessageException.POLICY_NOT_FOUND);

        if (policy.IsCancelled)
        {
            throw new ErrorOnValidationException(ResourcesMessageException.POLICY_CANCELLED_CANNOT_REGISTER_CLAIM);
        }

        if (!policy.IsInForce(request.OccurredOn))
        {
            throw new ErrorOnValidationException(ResourcesMessageException.CLAIM_DATE_OUTSIDE_POLICY_PERIOD);
        }

        var committedAmount = await claimsReadOnlyRepository.SumCommittedAmount(policy.Id, cancellationToken);
        policy.RegisterClaim(request.Amount, committedAmount);

        var claim = new Claim(policy.Id, request.Description, request.Amount, request.OccurredOn);

        await claimsWriteOnlyRepository.Add(claim);
        await unitOfWork.Commit(cancellationToken);
        logger.ClaimRegistered(claim.Id, policy.Id, claim.Amount);

        return new ResponseCreatedJson { Id = claim.Id };
    }

    private static void Validate(RequestClaimJson request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Description))
            errors.Add(ResourcesMessageException.DESCRIPTION_REQUIRED);
        else if (FieldLimits.Exceeds(request.Description, FieldLimits.Description))
            errors.Add(FieldLimits.DescriptionTooLong);

        if (request.Amount <= 0)
            errors.Add(ResourcesMessageException.CLAIM_AMOUNT_MUST_BE_POSITIVE);

        if (request.OccurredOn == default)
            errors.Add(ResourcesMessageException.CLAIM_OCCURRENCE_DATE_REQUIRED);
        else if (request.OccurredOn.Date > DateTime.UtcNow.Date)
            errors.Add(ResourcesMessageException.CLAIM_OCCURRENCE_DATE_IN_FUTURE);

        if (errors.Count > 0)
            throw new ErrorOnValidationException(errors);
    }
}
