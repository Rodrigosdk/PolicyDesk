using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Domain.Validations;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

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
        if (policyholderId == Guid.Empty)
            throw new DomainRuleException(ResourcesMessageException.POLICYHOLDER_REQUIRED);
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainRuleException(ResourcesMessageException.DESCRIPTION_REQUIRED);
        if (FieldLimits.Exceeds(description, FieldLimits.Description))
            throw new DomainRuleException(FieldLimits.DescriptionTooLong);
        if (coverageAmount <= 0)
            throw new DomainRuleException(ResourcesMessageException.COVERAGE_AMOUNT_MUST_BE_POSITIVE);
        if (!MoneyLimits.IsValid(coverageAmount))
            throw new DomainRuleException(MoneyLimits.CoverageAmountInvalidPrecision);
        if (premiumAmount <= 0)
            throw new DomainRuleException(ResourcesMessageException.PREMIUM_AMOUNT_MUST_BE_POSITIVE);
        if (!MoneyLimits.IsValid(premiumAmount))
            throw new DomainRuleException(MoneyLimits.PremiumAmountInvalidPrecision);
        if (startDate == default || endDate == default)
            throw new DomainRuleException(ResourcesMessageException.POLICY_DATES_REQUIRED);
        if (endDate.Date <= startDate.Date)
            throw new DomainRuleException(ResourcesMessageException.POLICY_END_DATE_MUST_BE_AFTER_START_DATE);

        PolicyholderId = policyholderId;
        Description = description.Trim();
        CoverageAmount = coverageAmount;
        PremiumAmount = premiumAmount;
        StartDate = startDate.Date;
        EndDate = endDate.Date;
    }

    // Usado pelo EF Core ao carregar do banco: não reaplica as regras de criação
    private Policy()
    {
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

    public void RegisterClaim(decimal amount, decimal committedAmount)
    {
        if (committedAmount + amount > CoverageAmount)
            throw new DomainRuleException(ResourcesMessageException.CLAIM_AMOUNT_EXCEEDS_COVERAGE);

        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainRuleException(ResourcesMessageException.POLICY_ALREADY_CANCELLED);

        Status = PolicyStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = CancelledAt;
    }
}
