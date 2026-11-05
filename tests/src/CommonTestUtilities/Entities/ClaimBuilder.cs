using Bogus;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;

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

    // Leva o sinistro até o status pedido passando apenas por transições válidas
    public static Claim BuildWithStatus(ClaimStatus status, Guid? policyId = null)
    {
        var claim = Build(policyId);
        if (status == ClaimStatus.Pending)
            return claim;

        claim.StartAnalysis();
        switch (status)
        {
            case ClaimStatus.Denied:
                claim.Deny("Not covered");
                break;
            case ClaimStatus.Approved:
                claim.Approve();
                break;
            case ClaimStatus.Paid:
                claim.Approve();
                claim.RegisterPayment();
                break;
        }

        return claim;
    }
}
