using System.Net;
using System.Net.Http.Json;
using CommonTestUtilities.Requests;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Api.IntegrationTests.Policyholders;

public abstract class PolicyholdersTestBase(PolicyDeskApplicationFactory factory) : BaseIntegrationTest(factory)
{
    protected const string RequestUri = "/api/segurados";

    protected async Task<Policyholder> RegisterPolicyholder(string? cpf = null, string? email = null)
    {
        var request = RequestPolicyholderJsonBuilder.Build(cpf, email);

        var response = await Post(RequestUri, request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await FindByCpf(request.Cpf);
    }

    protected Task<Policyholder> FindByCpf(string cpf) =>
        DbContext.Policyholders.IgnoreQueryFilters().AsNoTracking().SingleAsync(p => p.Cpf == cpf);

    protected Task<Policyholder> FindById(Guid id) =>
        DbContext.Policyholders.IgnoreQueryFilters().AsNoTracking().SingleAsync(p => p.Id == id);

    protected async Task<HttpResponseMessage> RegisterPolicy(Guid policyholderId)
    {
        var today = DateTime.UtcNow.Date;
        return await Post("/api/apolices", new RequestPolicyJson
        {
            SeguradoId = policyholderId,
            Descricao = "Active integration-test policy",
            ValorCobertura = 10000,
            ValorPremio = 100,
            InicioVigencia = today.AddDays(-1),
            FimVigencia = today.AddDays(1)
        });
    }
}
