using System.Net;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Enums;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Claims.Workflow;

public class ClaimWorkflowTests(PolicyDeskApplicationFactory factory) : ClaimsTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_PercorrerFluxoCompleto_Quando_SinistroForAprovadoEPago))]
    [Trait("Integration", "Claims - Fluxo")]
    public async Task Deve_PercorrerFluxoCompleto_Quando_SinistroForAprovadoEPago()
    {
        var claim = await RegisterClaimForNewPolicy();

        (await Post($"/api/sinistros/{claim.Id}/analise", new { })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await FindClaim(claim.Id)).Status.ShouldBe(ClaimStatus.InAnalysis);

        (await Post($"/api/sinistros/{claim.Id}/aprovacao", new { })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var approved = await FindClaim(claim.Id);
        approved.Status.ShouldBe(ClaimStatus.Approved);
        approved.ApprovedAt.ShouldNotBeNull();

        (await Post($"/api/sinistros/{claim.Id}/pagamento", new { })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var paid = await FindClaim(claim.Id);
        paid.Status.ShouldBe(ClaimStatus.Paid);
        paid.PaidAt.ShouldNotBeNull();
    }

    [Fact(DisplayName = nameof(Deve_NegarSinistro_Quando_EstiverEmAnaliseEMotivoForInformado))]
    [Trait("Integration", "Claims - Negação")]
    public async Task Deve_NegarSinistro_Quando_EstiverEmAnaliseEMotivoForInformado()
    {
        var claim = await RegisterClaimForNewPolicy();
        await Post($"/api/sinistros/{claim.Id}/analise", new { });

        var response = await Post(
            $"/api/sinistros/{claim.Id}/negacao",
            new RequestDenyClaimJson { Motivo = "  Not covered  " });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var saved = await FindClaim(claim.Id);
        saved.Status.ShouldBe(ClaimStatus.Denied);
        saved.DenialReason.ShouldBe("Not covered");
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_MotivoDeNegacaoForVazio))]
    [Trait("Integration", "Claims - Negação")]
    public async Task Deve_RetornarBadRequest_Quando_MotivoDeNegacaoForVazio()
    {
        var claim = await RegisterClaimForNewPolicy();
        await Post($"/api/sinistros/{claim.Id}/analise", new { });

        var response = await Post($"/api/sinistros/{claim.Id}/negacao", new RequestDenyClaimJson());

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await FindClaim(claim.Id)).Status.ShouldBe(ClaimStatus.InAnalysis);
    }

    [Theory(DisplayName = nameof(Deve_RetornarBadRequest_Quando_TransicaoForInvalida))]
    [Trait("Integration", "Claims - Fluxo")]
    [InlineData("aprovacao")]
    [InlineData("negacao")]
    [InlineData("pagamento")]
    public async Task Deve_RetornarBadRequest_Quando_TransicaoForInvalida(string action)
    {
        var claim = await RegisterClaimForNewPolicy();

        var response = await Post($"/api/sinistros/{claim.Id}/{action}", new RequestDenyClaimJson { Motivo = "Reason" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await FindClaim(claim.Id)).Status.ShouldBe(ClaimStatus.Pending);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_IniciarAnaliseDeSinistroJaEmAnalise))]
    [Trait("Integration", "Claims - Fluxo")]
    public async Task Deve_RetornarBadRequest_Quando_IniciarAnaliseDeSinistroJaEmAnalise()
    {
        var claim = await RegisterClaimForNewPolicy();
        await Post($"/api/sinistros/{claim.Id}/analise", new { });

        var response = await Post($"/api/sinistros/{claim.Id}/analise", new { });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Theory(DisplayName = nameof(Deve_RetornarNotFound_Quando_SinistroNaoExistir))]
    [Trait("Integration", "Claims - Fluxo")]
    [InlineData("analise")]
    [InlineData("aprovacao")]
    [InlineData("negacao")]
    [InlineData("pagamento")]
    public async Task Deve_RetornarNotFound_Quando_SinistroNaoExistir(string action)
    {
        var response = await Post(
            $"/api/sinistros/{Guid.NewGuid()}/{action}",
            new RequestDenyClaimJson { Motivo = "Reason" });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
