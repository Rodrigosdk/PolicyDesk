using Bogus;
using CommonTestUtilities.Time;
using PolicyDesk.Domain.Entities;

namespace CommonTestUtilities.Entities;

public static class PolicyBuilder
{
    public static Policy Build(
        Guid? policyholderId = null,
        string? description = null,
        decimal? coverageAmount = null,
        decimal? premiumAmount = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null) =>
        new Faker<Policy>()
            .CustomInstantiator(_ => (Policy)Activator.CreateInstance(typeof(Policy), nonPublic: true)!)
            .RuleFor(policy => policy.PolicyholderId, f => policyholderId ?? f.Random.Guid())
            .RuleFor(policy => policy.Description, f => description ?? f.Commerce.ProductName())
            .RuleFor(policy => policy.CoverageAmount, f => coverageAmount ?? f.Finance.Amount(1000, 100000))
            .RuleFor(policy => policy.PremiumAmount, f => premiumAmount ?? f.Finance.Amount(10, 1000))
            .RuleFor(policy => policy.StartDate, _ => startDate ?? BusinessClockBuilder.DefaultToday.AddDays(-1))
            .RuleFor(policy => policy.EndDate, _ => endDate ?? BusinessClockBuilder.DefaultToday.AddDays(30))
            .Generate();

    public static Policy BuildToCreate(
        Guid? policyholderId = null,
        string? description = null,
        decimal? coverageAmount = null,
        decimal? premiumAmount = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        var faker = new Faker();

        return Policy.Create(
            policyholderId ?? faker.Random.Guid(),
            description ?? faker.Commerce.ProductName(),
            coverageAmount ?? faker.Finance.Amount(1000, 100000),
            premiumAmount ?? faker.Finance.Amount(10, 1000),
            startDate ?? BusinessClockBuilder.DefaultToday.AddDays(-1),
            endDate ?? BusinessClockBuilder.DefaultToday.AddDays(30));
    }
}
