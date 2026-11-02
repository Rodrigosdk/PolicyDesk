using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.SeedWorks;

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
        Status = ClaimStatus.InAnalysis;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        Status = ClaimStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = ApprovedAt;
    }

    public void Deny(string reason)
    {
        Status = ClaimStatus.Denied;
        DenialReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RegisterPayment()
    {
        Status = ClaimStatus.Paid;
        PaidAt = DateTime.UtcNow;
        UpdatedAt = PaidAt;
    }
}
