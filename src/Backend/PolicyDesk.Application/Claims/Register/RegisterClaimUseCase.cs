using Microsoft.Extensions.Logging;
using PolicyDesk.Application.Logging;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Claims;
using PolicyDesk.Domain.Repositories.Policies;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Application.Claims.Register;

internal class RegisterClaimUseCase(
    IClaimsWriteOnlyRepository claimsWriteOnlyRepository,
    IClaimsReadOnlyRepository claimsReadOnlyRepository,
    IPoliciesUpdateOnlyRepository policiesUpdateOnlyRepository,
    IUnitOfWork unitOfWork,
    BusinessClock clock,
    ILogger<RegisterClaimUseCase> logger) : IRegisterClaimUseCase
{
    public async Task<ResponseCreatedJson> ExecuteAsync(Guid policyId, RequestClaimJson request, CancellationToken cancellationToken = default)
    {
        Validate(request, clock.Today);

        var policy = await policiesUpdateOnlyRepository.GetByIdForUpdateAsync(policyId, cancellationToken)
                     ?? throw new NotFoundException(ResourcesMessageException.POLICY_NOT_FOUND);

        if (policy.IsCancelled)
        {
            throw new ErrorOnValidationException(ResourcesMessageException.POLICY_CANCELLED_CANNOT_REGISTER_CLAIM);
        }

        if (!policy.IsInForce(request.DataOcorrencia))
        {
            throw new ErrorOnValidationException(ResourcesMessageException.CLAIM_DATE_OUTSIDE_POLICY_PERIOD);
        }

        var committedAmount = await claimsReadOnlyRepository.SumCommittedAmountAsync(policy.Id, cancellationToken);
        policy.RegisterClaim(request.Valor, committedAmount);

        var claim = Claim.Create(policy.Id, request.Descricao, request.Valor, request.DataOcorrencia);

        await claimsWriteOnlyRepository.AddAsync(claim);
        await unitOfWork.CommitAsync(cancellationToken);
        logger.ClaimRegistered(claim.Id, policy.Id, claim.Amount);

        return new ResponseCreatedJson { Id = claim.Id };
    }

    private static void Validate(RequestClaimJson request, DateOnly today)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Descricao))
            errors.Add(ResourcesMessageException.DESCRIPTION_REQUIRED);
        else if (FieldLimits.Exceeds(request.Descricao, FieldLimits.Description))
            errors.Add(FieldLimits.DescriptionTooLong);

        if (request.Valor <= 0)
            errors.Add(ResourcesMessageException.CLAIM_AMOUNT_MUST_BE_POSITIVE);
        else if (!MoneyLimits.IsValid(request.Valor))
            errors.Add(MoneyLimits.ClaimAmountInvalidPrecision);

        if (request.DataOcorrencia == default)
            errors.Add(ResourcesMessageException.CLAIM_OCCURRENCE_DATE_REQUIRED);
        else if (request.DataOcorrencia > today)
            errors.Add(ResourcesMessageException.CLAIM_OCCURRENCE_DATE_IN_FUTURE);

        if (errors.Count > 0)
            throw new ErrorOnValidationException(errors);
    }
}
