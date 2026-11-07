using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Domain.Entities;

public class Claim : Entity
{
    public Claim(Guid policyId, string description, decimal amount, DateTime occurredOn)
    {
        if (policyId == Guid.Empty)
            throw new DomainRuleException(ResourcesMessageException.POLICY_REQUIRED);
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainRuleException(ResourcesMessageException.DESCRIPTION_REQUIRED);
        if (FieldLimits.Exceeds(description, FieldLimits.Description))
            throw new DomainRuleException(FieldLimits.DescriptionTooLong);
        if (amount <= 0)
            throw new DomainRuleException(ResourcesMessageException.CLAIM_AMOUNT_MUST_BE_POSITIVE);
        if (!MoneyLimits.IsValid(amount))
            throw new DomainRuleException(MoneyLimits.ClaimAmountInvalidPrecision);
        if (occurredOn == default)
            throw new DomainRuleException(ResourcesMessageException.CLAIM_OCCURRENCE_DATE_REQUIRED);

        PolicyId = policyId;
        Description = description.Trim();
        Amount = amount;
        OccurredOn = occurredOn.Date;
    }

    // Usado pelo EF Core ao carregar do banco: não reaplica as regras de criação
    private Claim()
    {
    }

    public Guid PolicyId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public ClaimStatus Status { get; private set; } = ClaimStatus.Pending;
    public DateTime? ApprovedAt { get; private set; }
    public string? DenialReason { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public void StartAnalysis()
    {
        EnsureStatus(ClaimStatus.Pending, ResourcesMessageException.CLAIM_MUST_BE_PENDING_FOR_ANALYSIS);

        Status = ClaimStatus.InAnalysis;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        EnsureStatus(ClaimStatus.InAnalysis, ResourcesMessageException.CLAIM_MUST_BE_IN_ANALYSIS_FOR_APPROVAL);

        Status = ClaimStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = ApprovedAt;
    }

    public void Deny(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainRuleException(ResourcesMessageException.CLAIM_DENIAL_REASON_REQUIRED);
        if (FieldLimits.Exceeds(reason, FieldLimits.DenialReason))
            throw new DomainRuleException(FieldLimits.DenialReasonTooLong);

        EnsureStatus(ClaimStatus.InAnalysis, ResourcesMessageException.CLAIM_MUST_BE_IN_ANALYSIS_FOR_DENIAL);

        Status = ClaimStatus.Denied;
        DenialReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RegisterPayment()
    {
        EnsureStatus(ClaimStatus.Approved, ResourcesMessageException.CLAIM_MUST_BE_APPROVED_FOR_PAYMENT);

        Status = ClaimStatus.Paid;
        PaidAt = DateTime.UtcNow;
        UpdatedAt = PaidAt;
    }

    private void EnsureStatus(ClaimStatus expected, string error)
    {
        if (Status != expected)
            throw new DomainRuleException(error);
    }
}
