using Bogus;
using PolicyDesk.Communication.Requests;

namespace CommonTestUtilities.Requests;

public static class RequestPolicyJsonBuilder
{
    public static RequestPolicyJson Build(
        Guid? policyholderId = null,
        string? description = null,
        decimal? coverageAmount = null,
        decimal? premiumAmount = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var faker = new Faker();
        var today = DateTime.UtcNow.Date;
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
