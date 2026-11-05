using System.Net;
using System.Net.Http.Json;
using CommonTestUtilities.Requests;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Communication.Requests;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Exception;
using Shouldly;
using Policy = PolicyDesk.Domain.Entities.Policy;

namespace PolicyDesk.Api.IntegrationTests.Claims.Register;

public class RegisterClaimTests(PolicyDeskApplicationFactory factory) : ClaimsTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_CadastrarSinistroPendente_Quando_RequisicaoForValida))]
    [Trait("Integration", "Claims - Cadastro")]
    public async Task Deve_CadastrarSinistroPendente_Quando_RequisicaoForValida()
    {
        var policy = await RegisterPolicyForNewHolder();
        var request = RequestClaimJsonBuilder.Build("  Broken window  ", 250);

        var response = await Post($"/api/apolices/{policy.Id}/sinistros", request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ResponseCreatedJson>();
        created.ShouldNotBeNull();
        var saved = await FindClaim(created.Id);
        saved.PolicyId.ShouldBe(policy.Id);
        saved.Description.ShouldBe("Broken window");
        saved.Amount.ShouldBe(250);
        saved.OccurredOn.ShouldBe(request.OccurredOn.Date);
        saved.Status.ShouldBe(ClaimStatus.Pending);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNotFound_Quando_ApoliceNaoExistir))]
    [Trait("Integration", "Claims - Cadastro")]
    public async Task Deve_RetornarNotFound_Quando_ApoliceNaoExistir()
    {
        var response = await Post($"/api/apolices/{Guid.NewGuid()}/sinistros", RequestClaimJsonBuilder.Build());

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_ValorForInvalido))]
    [Trait("Integration", "Claims - Cadastro")]
    public async Task Deve_RetornarBadRequest_Quando_ValorForInvalido()
    {
        var policy = await RegisterPolicyForNewHolder();

        var response = await Post(
            $"/api/apolices/{policy.Id}/sinistros",
            RequestClaimJsonBuilder.Build(amount: 0));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync())
            .ShouldContain(ResourcesMessageException.CLAIM_AMOUNT_MUST_BE_POSITIVE);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_ValorExcederCobertura))]
    [Trait("Integration", "Claims - Cadastro")]
    public async Task Deve_RetornarBadRequest_Quando_ValorExcederCobertura()
    {
        var policyholder = await RegisterPolicyholder();
        var policy = await RegisterPolicyForHolder(
            policyholder,
            RequestPolicyJsonBuilder.Build(policyholder.Id, coverageAmount: 1000));

        var response = await Post(
            $"/api/apolices/{policy.Id}/sinistros",
            RequestClaimJsonBuilder.Build(amount: 1000.01m));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync())
            .ShouldContain(ResourcesMessageException.CLAIM_AMOUNT_EXCEEDS_COVERAGE);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_DataEstiverForaDaVigencia))]
    [Trait("Integration", "Claims - Cadastro")]
    public async Task Deve_RetornarBadRequest_Quando_DataEstiverForaDaVigencia()
    {
        var policy = await RegisterPolicyForNewHolder();

        var response = await Post(
            $"/api/apolices/{policy.Id}/sinistros",
            RequestClaimJsonBuilder.Build(occurredOn: DateTime.UtcNow.Date.AddDays(-30)));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync())
            .ShouldContain(ResourcesMessageException.CLAIM_DATE_OUTSIDE_POLICY_PERIOD);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequestSemGravar_Quando_DataDeOcorrenciaForFutura))]
    [Trait("Integration", "Claims - Cadastro")]
    public async Task Deve_RetornarBadRequestSemGravar_Quando_DataDeOcorrenciaForFutura()
    {
        var policy = await RegisterPolicyForNewHolder();

        var response = await Post(
            $"/api/apolices/{policy.Id}/sinistros",
            RequestClaimJsonBuilder.Build(amount: 100, occurredOn: DateTime.UtcNow.Date.AddDays(1)));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync())
            .ShouldContain(ResourcesMessageException.CLAIM_OCCURRENCE_DATE_IN_FUTURE);
        (await DbContext.Claims.AnyAsync(claim => claim.PolicyId == policy.Id)).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_ApoliceEstiverCancelada))]
    [Trait("Integration", "Claims - Cadastro")]
    public async Task Deve_RetornarBadRequest_Quando_ApoliceEstiverCancelada()
    {
        var policy = await RegisterPolicyForNewHolder();
        (await Post($"/api/apolices/{policy.Id}/cancelamento", new { }))
            .StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var response = await Post($"/api/apolices/{policy.Id}/sinistros", RequestClaimJsonBuilder.Build(amount: 100));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync())
            .ShouldContain(ResourcesMessageException.POLICY_CANCELLED_CANNOT_REGISTER_CLAIM);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequestSemGravar_Quando_SomaDosSinistrosExcederCobertura))]
    [Trait("Integration", "Claims - Cadastro")]
    public async Task Deve_RetornarBadRequestSemGravar_Quando_SomaDosSinistrosExcederCobertura()
    {
        var policy = await RegisterPolicyWithCoverage(10000);
        await RegisterClaimForPolicy(policy, RequestClaimJsonBuilder.Build(amount: 8000));

        var response = await Post(
            $"/api/apolices/{policy.Id}/sinistros",
            RequestClaimJsonBuilder.Build(amount: 8000));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync())
            .ShouldContain(ResourcesMessageException.CLAIM_AMOUNT_EXCEEDS_COVERAGE);
        (await DbContext.Claims.CountAsync(claim => claim.PolicyId == policy.Id)).ShouldBe(1);
    }

    [Fact(DisplayName = nameof(Deve_LiberarSaldo_Quando_SinistroForNegado))]
    [Trait("Integration", "Claims - Cadastro")]
    public async Task Deve_LiberarSaldo_Quando_SinistroForNegado()
    {
        var policy = await RegisterPolicyWithCoverage(10000);
        var denied = await RegisterClaimForPolicy(policy, RequestClaimJsonBuilder.Build(amount: 8000));
        await Post($"/api/sinistros/{denied.Id}/analise", new { });
        await Post($"/api/sinistros/{denied.Id}/negacao", new RequestDenyClaimJson { Motivo = "Not covered" });

        var response = await Post(
            $"/api/apolices/{policy.Id}/sinistros",
            RequestClaimJsonBuilder.Build(amount: 10000));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact(DisplayName = nameof(Deve_AceitarApenasUmSinistro_Quando_CadastrosSimultaneosExcederemCobertura))]
    [Trait("Integration", "Claims - Concorrência")]
    public async Task Deve_AceitarApenasUmSinistro_Quando_CadastrosSimultaneosExcederemCobertura()
    {
        var policy = await RegisterPolicyWithCoverage(10000);

        var responses = await Task.WhenAll(
            Post($"/api/apolices/{policy.Id}/sinistros", RequestClaimJsonBuilder.Build(amount: 6000)),
            Post($"/api/apolices/{policy.Id}/sinistros", RequestClaimJsonBuilder.Build(amount: 6000)));

        responses.Count(response => response.StatusCode == HttpStatusCode.Created).ShouldBe(1);
        responses.ShouldContain(response =>
            response.StatusCode == HttpStatusCode.Conflict || response.StatusCode == HttpStatusCode.BadRequest);
        (await DbContext.Claims.Where(claim => claim.PolicyId == policy.Id).SumAsync(claim => claim.Amount))
            .ShouldBe(6000);
    }

    private async Task<Policy> RegisterPolicyWithCoverage(decimal coverageAmount)
    {
        var policyholder = await RegisterPolicyholder();
        return await RegisterPolicyForHolder(
            policyholder,
            RequestPolicyJsonBuilder.Build(policyholder.Id, coverageAmount: coverageAmount));
    }
}
