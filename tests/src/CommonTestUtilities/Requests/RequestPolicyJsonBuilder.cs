using Bogus;
using CommonTestUtilities.Time;
using PolicyDesk.Communication.Requests;

namespace CommonTestUtilities.Requests;

public static class RequestPolicyJsonBuilder
{
    public static RequestPolicyJson Build(
        Guid? policyholderId = null,
        string? description = null,
        decimal? coverageAmount = null,
        decimal? premiumAmount = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null) =>
        new Faker<RequestPolicyJson>()
            .RuleFor(request => request.SeguradoId, f => policyholderId ?? f.Random.Guid())
            .RuleFor(request => request.Descricao, f => description ?? f.Commerce.ProductName())
            .RuleFor(request => request.ValorCobertura, f => coverageAmount ?? f.Finance.Amount(1000, 100000))
            .RuleFor(request => request.ValorPremio, f => premiumAmount ?? f.Finance.Amount(10, 1000))
            .RuleFor(request => request.InicioVigencia, _ => startDate ?? BusinessClockBuilder.DefaultToday.AddDays(-1))
            .RuleFor(request => request.FimVigencia, _ => endDate ?? BusinessClockBuilder.DefaultToday.AddDays(30))
            .Generate();
}
