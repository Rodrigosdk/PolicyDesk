using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Domain.Entities;

public class Claim(
    Guid policyId,
    string description,
    decimal amount,
    DateTime occurredOn
    ) : Entity
{
    public Guid PolicyId { get; private set; } = policyId;
    public string Description { get; private set; } = description.Trim();
    public decimal Amount { get; private set; } = amount;
    public DateTime OccurredOn { get; private set; } = occurredOn.Date;
    public ClaimStatus Status { get; private set; } = ClaimStatus.Pending;
    public DateTime? ApprovedAt { get; private set; }
    public string? DenialReason { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public bool IsApproved => Status == ClaimStatus.Approved;

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
            throw new ErrorOnValidationException(ResourcesMessageException.CLAIM_DENIAL_REASON_REQUIRED);

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
            throw new ErrorOnValidationException(error);
    }
}
