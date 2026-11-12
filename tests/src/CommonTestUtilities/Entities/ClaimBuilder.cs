using Bogus;
using CommonTestUtilities.Time;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;

namespace CommonTestUtilities.Entities;

public static class ClaimBuilder
{
    public static Claim Build(
        Guid? policyId = null,
        string? description = null,
        decimal? amount = null,
        DateOnly? occurredOn = null) =>
        new Faker<Claim>()
            .CustomInstantiator(_ => (Claim)Activator.CreateInstance(typeof(Claim), nonPublic: true)!)
            .RuleFor(claim => claim.PolicyId, f => policyId ?? f.Random.Guid())
            .RuleFor(claim => claim.Description, f => description ?? f.Lorem.Sentence())
            .RuleFor(claim => claim.Amount, f => amount ?? f.Finance.Amount(1, 10000))
            .RuleFor(claim => claim.OccurredOn, _ => occurredOn ?? BusinessClockBuilder.DefaultToday)
            .Generate();

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
