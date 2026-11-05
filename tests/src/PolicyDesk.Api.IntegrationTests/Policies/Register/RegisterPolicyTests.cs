using PolicyDesk.Domain.Validations;
using System.Net;
using System.Net.Http.Json;
using CommonTestUtilities.Requests;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Communication.Responses;
using PolicyDesk.Exception;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Policies.Register;

public class RegisterPolicyTests(PolicyDeskApplicationFactory factory) : PoliciesTestBase(factory)
{
    [Fact(DisplayName = nameof(Deve_CadastrarApoliceAtiva_Quando_RequisicaoForValida))]
    [Trait("Integration", "Policies - Cadastro")]
    public async Task Deve_CadastrarApoliceAtiva_Quando_RequisicaoForValida()
    {
        var policyholder = await RegisterPolicyholder();
        var request = RequestPolicyJsonBuilder.Build(
            policyholder.Id,
            description: "  Home coverage  ",
            coverageAmount: 20000,
            premiumAmount: 250);

        var response = await Post("/api/apolices", request);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ResponseCreatedJson>();
        created.ShouldNotBeNull();
        var saved = await DbContext.Policies.FindAsync(created.Id);
        saved.ShouldNotBeNull();
        saved.PolicyholderId.ShouldBe(policyholder.Id);
        saved.Description.ShouldBe("Home coverage");
        saved.CoverageAmount.ShouldBe(request.CoverageAmount);
        saved.PremiumAmount.ShouldBe(request.PremiumAmount);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_ValorCoberturaForInvalido))]
    [Trait("Integration", "Policies - Cadastro")]
    public async Task Deve_RetornarBadRequest_Quando_ValorCoberturaForInvalido()
    {
        var policyholder = await RegisterPolicyholder();

        var response = await Post(
            "/api/apolices",
            RequestPolicyJsonBuilder.Build(policyholder.Id, coverageAmount: 0));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync())
            .ShouldContain(ResourcesMessageException.COVERAGE_AMOUNT_MUST_BE_POSITIVE);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNotFound_Quando_SeguradoNaoExistir))]
    [Trait("Integration", "Policies - Cadastro")]
    public async Task Deve_RetornarNotFound_Quando_SeguradoNaoExistir()
    {
        var response = await Post("/api/apolices", RequestPolicyJsonBuilder.Build());

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequestSemGravar_Quando_SeguradoEstiverInativo))]
    [Trait("Integration", "Policies - Cadastro")]
    public async Task Deve_RetornarBadRequestSemGravar_Quando_SeguradoEstiverInativo()
    {
        var policyholder = await RegisterPolicyholder();
        (await Delete($"{RequestUri}/{policyholder.Id}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var response = await Post("/api/apolices", RequestPolicyJsonBuilder.Build(policyholder.Id));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain(ResourcesMessageException.POLICYHOLDER_INACTIVE);
        (await DbContext.Policies.AnyAsync(policy => policy.PolicyholderId == policyholder.Id)).ShouldBeFalse();
    }

    [Fact(DisplayName = nameof(Deve_RetornarBadRequest_Quando_DataFinalNaoForPosteriorAInicial))]
    [Trait("Integration", "Policies - Cadastro")]
    public async Task Deve_RetornarBadRequest_Quando_DataFinalNaoForPosteriorAInicial()
    {
        var policyholder = await RegisterPolicyholder();
        var today = DateTime.UtcNow.Date;

        var response = await Post(
            "/api/apolices",
            RequestPolicyJsonBuilder.Build(policyholder.Id, startDate: today, endDate: today));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync())
            .ShouldContain(ResourcesMessageException.POLICY_END_DATE_MUST_BE_AFTER_START_DATE);
    }


    [Fact(DisplayName = nameof(Deve_RetornarBadRequestSemGravar_Quando_DescricaoExcederOLimite))]
    [Trait("Integration", "Policies - Cadastro")]
    public async Task Deve_RetornarBadRequestSemGravar_Quando_DescricaoExcederOLimite()
    {
        var policyholder = await RegisterPolicyholder();

        var response = await Post(
            "/api/apolices",
            RequestPolicyJsonBuilder.Build(policyholder.Id, description: new string('a', FieldLimits.Description + 1)));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain(FieldLimits.DescriptionTooLong);
        (await DbContext.Policies.AnyAsync(policy => policy.PolicyholderId == policyholder.Id)).ShouldBeFalse();
    }
}
