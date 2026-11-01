using Bogus;
using PolicyDesk.Communication.Requests;

namespace CommonTestUtilities.Requests;

public static class RequestClaimJsonBuilder
{
    public static RequestClaimJson Build(
        string? description = null,
        decimal? amount = null,
        DateTime? occurredOn = null)
    {
        var faker = new Faker();
        return new RequestClaimJson
        {
            Description = description ?? faker.Lorem.Sentence(),
            Amount = amount ?? faker.Finance.Amount(1, 10000),
            OccurredOn = occurredOn ?? DateTime.UtcNow.Date
        };
    }
}
