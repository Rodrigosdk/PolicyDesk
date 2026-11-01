using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.SeedWorks;

namespace PolicyDesk.Domain.Entities;

public class Policy : Entity
{
    public Policy(
        Guid policyholderId,
        string description,
        decimal coverageAmount,
        decimal premiumAmount,
        DateTime startDate,
        DateTime endDate)
    {
        PolicyholderId = policyholderId;
        Description = description.Trim();
        CoverageAmount = coverageAmount;
        PremiumAmount = premiumAmount;
        StartDate = startDate.Date;
        EndDate = endDate.Date;
    }

    public Guid PolicyholderId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal CoverageAmount { get; private set; }
    public decimal PremiumAmount { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public PolicyStatus Status { get; private set; } = PolicyStatus.Active;
    public DateTime? CancelledAt { get; private set; }

    public bool IsCancelled => Status == PolicyStatus.Cancelled;

    public bool IsInForce(DateTime date) => !IsCancelled && date.Date >= StartDate && date.Date <= EndDate;

    public void Cancel()
    {
        Status = PolicyStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = CancelledAt;
    }
}
