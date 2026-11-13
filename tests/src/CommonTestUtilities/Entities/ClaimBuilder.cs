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

    public static Claim BuildToCreate(
        Guid? policyId = null,
        string? description = null,
        decimal? amount = null,
        DateOnly? occurredOn = null)
    {
        var faker = new Faker();

        return Claim.Create(
            policyId ?? faker.Random.Guid(),
            description ?? faker.Lorem.Sentence(),
            amount ?? faker.Finance.Amount(1, 10000),
            occurredOn ?? BusinessClockBuilder.DefaultToday);
    }

    // Leva o sinistro até o status pedido passando apenas por transições válidas
    public static Claim BuildWithStatus(ClaimStatus status, Guid? policyId = null)
    {
        var claim = Build(policyId);

        switch (status)
        {
            case ClaimStatus.Pending:
                break;
            case ClaimStatus.InAnalysis:
                claim.StartAnalysis();
                break;
            case ClaimStatus.Approved:
                claim.StartAnalysis();
                claim.Approve();
                break;
            case ClaimStatus.Denied:
                claim.StartAnalysis();
                claim.Deny("Not covered");
                break;
            case ClaimStatus.Paid:
                claim.StartAnalysis();
                claim.Approve();
                claim.RegisterPayment();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        return claim;
    }
}
