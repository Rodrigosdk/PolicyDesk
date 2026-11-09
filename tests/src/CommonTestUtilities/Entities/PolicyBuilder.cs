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
        DateOnly? endDate = null)
    {
        var faker = new Faker();
        var today = BusinessClockBuilder.DefaultToday;
        return new Policy(
            policyholderId ?? Guid.NewGuid(),
            description ?? faker.Commerce.ProductName(),
            coverageAmount ?? faker.Finance.Amount(1000, 100000),
            premiumAmount ?? faker.Finance.Amount(10, 1000),
            startDate ?? today.AddDays(-1),
            endDate ?? today.AddDays(30));
    }
}
