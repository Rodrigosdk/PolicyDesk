using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using PolicyDesk.Application.Claims.Approve;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Claims.Workflow;

public class ClaimConcurrencyTests(PolicyDeskApplicationFactory factory) : ClaimsTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_RetornarConflict_Quando_OcorrerConflitoDeConcorrencia))]
    [Trait("Integration", "Claims - Concorrência")]
    public async Task Deve_RetornarConflict_Quando_OcorrerConflitoDeConcorrencia()
    {
        // Substitui o caso de uso por um que simula o conflito detectado no Commit
        var client = Factory
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
                services.AddScoped<IApproveClaimUseCase, ConflictingApproveClaimUseCase>()))
            .CreateClient();

        var response = await client.PostAsync($"/api/sinistros/{Guid.NewGuid()}/aprovacao", null);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await response.Content.ReadAsStringAsync()).ShouldContain(ResourcesMessageException.CONCURRENCY_CONFLICT);
    }

    [Fact(DisplayName = nameof(Deve_AceitarApenasUmaDecisao_Quando_AprovarENegarAoMesmoTempo))]
    [Trait("Integration", "Claims - Concorrência")]
    public async Task Deve_AceitarApenasUmaDecisao_Quando_AprovarENegarAoMesmoTempo()
    {
        var claim = await RegisterClaimForNewPolicy();
        await Post($"/api/sinistros/{claim.Id}/analise", new { });

        var responses = await Task.WhenAll(
            Post($"/api/sinistros/{claim.Id}/aprovacao", new { }),
            Post($"/api/sinistros/{claim.Id}/negacao", new RequestDenyClaimJson { Motivo = "Not covered" }));

        // Dependendo do momento, a perdedora recebe 409 (gravaram juntas) ou 400 (já tinha decisão);
        // o que nunca pode acontecer é as duas serem aceitas
        responses.Count(response => response.StatusCode == HttpStatusCode.NoContent).ShouldBe(1);
        responses.ShouldContain(response =>
            response.StatusCode == HttpStatusCode.Conflict || response.StatusCode == HttpStatusCode.BadRequest);
        var saved = await FindClaim(claim.Id);
        if (saved.Status == ClaimStatus.Approved)
            saved.DenialReason.ShouldBeNull();
        else
        {
            saved.Status.ShouldBe(ClaimStatus.Denied);
            saved.ApprovedAt.ShouldBeNull();
        }
    }

    [Fact(DisplayName = nameof(Deve_RegistrarUmUnicoPagamento_Quando_PagarDuasVezesAoMesmoTempo))]
    [Trait("Integration", "Claims - Concorrência")]
    public async Task Deve_RegistrarUmUnicoPagamento_Quando_PagarDuasVezesAoMesmoTempo()
    {
        var claim = await RegisterClaimForNewPolicy();
        await Post($"/api/sinistros/{claim.Id}/analise", new { });
        await Post($"/api/sinistros/{claim.Id}/aprovacao", new { });

        var responses = await Task.WhenAll(
            Post($"/api/sinistros/{claim.Id}/pagamento", new { }),
            Post($"/api/sinistros/{claim.Id}/pagamento", new { }));

        responses.Count(response => response.StatusCode == HttpStatusCode.NoContent).ShouldBe(1);
        (await FindClaim(claim.Id)).Status.ShouldBe(ClaimStatus.Paid);
    }

    private sealed class ConflictingApproveClaimUseCase : IApproveClaimUseCase
    {
        public Task Execute(Guid claimId, CancellationToken cancellationToken = default) =>
            throw new ConflictException(ResourcesMessageException.CONCURRENCY_CONFLICT);
    }
}
