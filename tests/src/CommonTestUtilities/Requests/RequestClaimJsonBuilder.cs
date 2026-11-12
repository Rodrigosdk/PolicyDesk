using Bogus;
using CommonTestUtilities.Time;
using PolicyDesk.Communication.Requests;

namespace CommonTestUtilities.Requests;

public static class RequestClaimJsonBuilder
{
    public static RequestClaimJson Build(
        string? description = null,
        decimal? amount = null,
        DateOnly? occurredOn = null) =>
        new Faker<RequestClaimJson>()
            .RuleFor(request => request.Descricao, f => description ?? f.Lorem.Sentence())
            .RuleFor(request => request.Valor, f => amount ?? f.Finance.Amount(1, 10000))
            .RuleFor(request => request.DataOcorrencia, _ => occurredOn ?? BusinessClockBuilder.DefaultToday)
            .Generate();
}
