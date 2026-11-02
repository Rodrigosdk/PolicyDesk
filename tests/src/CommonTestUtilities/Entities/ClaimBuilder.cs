using Bogus;
using PolicyDesk.Domain.Entities;

namespace CommonTestUtilities.Entities;

public static class ClaimBuilder
{
    public static Claim Build(
        Guid? policyId = null,
        string? description = null,
        decimal? amount = null,
        DateTime? occurredOn = null)
    {
        var faker = new Faker();
        return new Claim(
            policyId ?? Guid.NewGuid(),
            description ?? faker.Lorem.Sentence(),
            amount ?? faker.Finance.Amount(1, 10000),
            occurredOn ?? DateTime.UtcNow.Date);
    }
}
