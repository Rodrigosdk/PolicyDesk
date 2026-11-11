using System.Net;
using System.Net.Http.Json;
using CommonTestUtilities.Requests;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Api.IntegrationTests.Policies;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;
using Claim = PolicyDesk.Domain.Entities.Claim;
using Policy = PolicyDesk.Domain.Entities.Policy;

namespace PolicyDesk.Api.IntegrationTests.Claims;

public abstract class ClaimsTestBase(PolicyDeskApplicationFactory factory) : PoliciesTestBase(factory)
{
    protected async Task<Claim> RegisterClaimForPolicy(Policy policy, RequestClaimJson? request = null)
    {
        var response = await Post(
            $"/api/apolices/{policy.Id}/sinistros",
            request ?? RequestClaimJsonBuilder.Build(amount: 100));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ResponseCreatedJson>(TestJson.Options);
        Assert.NotNull(created);
        return await FindClaim(created.Id);
    }

    protected async Task<Claim> RegisterClaimForNewPolicy(RequestClaimJson? request = null)
    {
        var policy = await RegisterPolicyForNewHolder();
        return await RegisterClaimForPolicy(policy, request);
    }

    protected Task<Claim> FindClaim(Guid id) =>
        DbContext.Claims.AsNoTracking().SingleAsync(claim => claim.Id == id);
}
