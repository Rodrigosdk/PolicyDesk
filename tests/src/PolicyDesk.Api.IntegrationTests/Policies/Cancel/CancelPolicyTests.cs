using System.Net;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Domain.Enums;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Policies.Cancel;

public class CancelPolicyTests(PolicyDeskApplicationFactory factory) : PoliciesTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_CancelarApolice_Quando_ApoliceEstiverAtiva))]
    [Trait("Integration", "Policies - Cancelamento")]
    public async Task Deve_CancelarApolice_Quando_ApoliceEstiverAtiva()
    {
        var policy = await RegisterPolicyForNewHolder();

        var response = await Post($"/api/apolices/{policy.Id}/cancelamento", new { });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var saved = await DbContext.Policies.AsNoTracking().SingleAsync(item => item.Id == policy.Id);
        saved.Status.ShouldBe(PolicyStatus.Cancelled);
        saved.CancelledAt.ShouldNotBeNull();
        saved.UpdatedAt.ShouldBe(saved.CancelledAt);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNotFound_Quando_ApoliceNaoExistir))]
    [Trait("Integration", "Policies - Cancelamento")]
    public async Task Deve_RetornarNotFound_Quando_ApoliceNaoExistir()
    {
        var response = await Post($"/api/apolices/{Guid.NewGuid()}/cancelamento", new { });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_ApoliceJaEstiverCancelada))]
    [Trait("Integration", "Policies - Cancelamento")]
    public async Task Deve_RetornarBadRequest_Quando_ApoliceJaEstiverCancelada()
    {
        var policy = await RegisterPolicyForNewHolder();
        (await Post($"/api/apolices/{policy.Id}/cancelamento", new { }))
            .StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var response = await Post($"/api/apolices/{policy.Id}/cancelamento", new { });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
