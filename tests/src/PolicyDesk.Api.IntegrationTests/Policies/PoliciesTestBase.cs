using System.Net;
using System.Net.Http.Json;
using CommonTestUtilities.Requests;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Api.IntegrationTests.Policyholders;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;
using Policy = PolicyDesk.Domain.Entities.Policy;
using Policyholder = PolicyDesk.Domain.Entities.Policyholders;

namespace PolicyDesk.Api.IntegrationTests.Policies;

public abstract class PoliciesTestBase(PolicyDeskApplicationFactory factory) : PolicyholdersTestBase(factory)
{
    protected async Task<Policy> RegisterPolicyForHolder(Policyholder policyholder, RequestPolicyJson? request = null)
    {
        var response = await Post(
            "/api/apolices",
            request ?? RequestPolicyJsonBuilder.Build(policyholder.Id));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ResponseCreatedJson>();
        Assert.NotNull(created);
        return await DbContext.Policies.AsNoTracking().SingleAsync(policy => policy.Id == created.Id);
    }

    protected async Task<Policy> RegisterPolicyForNewHolder(RequestPolicyJson? request = null)
    {
        var policyholder = await RegisterPolicyholder();
        return await RegisterPolicyForHolder(policyholder, request);
    }
}
