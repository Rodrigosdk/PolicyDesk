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
        DateOnly? endDate = null)
    {
        var faker = new Faker();
        var today = BusinessClockBuilder.DefaultToday;
        return new RequestPolicyJson
        {
            SeguradoId = policyholderId ?? Guid.NewGuid(),
            Descricao = description ?? faker.Commerce.ProductName(),
            ValorCobertura = coverageAmount ?? faker.Finance.Amount(1000, 100000),
            ValorPremio = premiumAmount ?? faker.Finance.Amount(10, 1000),
            InicioVigencia = startDate ?? today.AddDays(-1),
            FimVigencia = endDate ?? today.AddDays(30)
        };
    }
}
