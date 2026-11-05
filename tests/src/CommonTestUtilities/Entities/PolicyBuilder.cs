using Bogus;
using PolicyDesk.Domain.Entities;

namespace CommonTestUtilities.Entities;

public static class PolicyBuilder
{
    public static Policy Build(
        Guid? policyholderId = null,
        string? description = null,
        decimal? coverageAmount = null,
        decimal? premiumAmount = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var faker = new Faker();
        var today = DateTime.UtcNow.Date;
        return new Policy(
            policyholderId ?? Guid.NewGuid(),
            description ?? faker.Commerce.ProductName(),
            coverageAmount ?? faker.Finance.Amount(1000, 100000),
            premiumAmount ?? faker.Finance.Amount(10, 1000),
            startDate ?? today.AddDays(-1),
            endDate ?? today.AddDays(30));
    }
}
