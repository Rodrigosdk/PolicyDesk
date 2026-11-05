using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using PolicyDesk.Api.Errors;
using PolicyDesk.Application.Claims.Approve;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Errors;

public class ErrorResponseTests(PolicyDeskApplicationFactory factory) : IClassFixture<PolicyDeskApplicationFactory>
{
    private const string ProblemJson = "application/problem+json";

    [Fact(DisplayName = nameof(Deve_RetornarProblemDetailsComMensagem_Quando_RegraDeDominioForViolada))]
    [Trait("Integration", "Erros - Formato único")]
    public async Task Deve_RetornarProblemDetailsComMensagem_Quando_RegraDeDominioForViolada()
    {
        var client = CreateClientThrowing(new DomainRuleException(ResourcesMessageException.INVALID_PHONE));

        var response = await client.PostAsync($"/api/sinistros/{Guid.NewGuid()}/aprovacao", null);

        var problem = await ShouldBeProblem(response, HttpStatusCode.BadRequest);
        Errors(problem).ShouldBe([ResourcesMessageException.INVALID_PHONE]);
    }

    [Fact(DisplayName = nameof(Deve_RetornarTodosOsErros_Quando_ValidacaoFalharComVariasMensagens))]
    [Trait("Integration", "Erros - Formato único")]
    public async Task Deve_RetornarTodosOsErros_Quando_ValidacaoFalharComVariasMensagens()
    {
        var client = CreateClientThrowing(new ErrorOnValidationException(
            [ResourcesMessageException.DESCRIPTION_REQUIRED, ResourcesMessageException.CLAIM_AMOUNT_MUST_BE_POSITIVE]));

        var response = await client.PostAsync($"/api/sinistros/{Guid.NewGuid()}/aprovacao", null);

        var problem = await ShouldBeProblem(response, HttpStatusCode.BadRequest);
        Errors(problem).ShouldBe([ResourcesMessageException.DESCRIPTION_REQUIRED, ResourcesMessageException.CLAIM_AMOUNT_MUST_BE_POSITIVE]);
    }

    [Fact(DisplayName = nameof(Deve_RetornarNotFoundNoMesmoFormato_Quando_RecursoNaoExistir))]
    [Trait("Integration", "Erros - Formato único")]
    public async Task Deve_RetornarNotFoundNoMesmoFormato_Quando_RecursoNaoExistir()
    {
        var response = await factory.CreateClient().PostAsync($"/api/sinistros/{Guid.NewGuid()}/aprovacao", null);

        var problem = await ShouldBeProblem(response, HttpStatusCode.NotFound);
        Errors(problem).ShouldBe([ResourcesMessageException.CLAIM_NOT_FOUND]);
    }

    [Fact(DisplayName = nameof(Deve_RetornarConflictNoMesmoFormato_Quando_HouverConflitoDeConcorrencia))]
    [Trait("Integration", "Erros - Formato único")]
    public async Task Deve_RetornarConflictNoMesmoFormato_Quando_HouverConflitoDeConcorrencia()
    {
        var client = CreateClientThrowing(new ConflictException(ResourcesMessageException.CONCURRENCY_CONFLICT));

        var response = await client.PostAsync($"/api/sinistros/{Guid.NewGuid()}/aprovacao", null);

        var problem = await ShouldBeProblem(response, HttpStatusCode.Conflict);
        Errors(problem).ShouldBe([ResourcesMessageException.CONCURRENCY_CONFLICT]);
    }

    [Fact(DisplayName = nameof(Deve_RetornarListaDeErros_Quando_CorpoDaRequisicaoForMalformado))]
    [Trait("Integration", "Erros - Formato único")]
    public async Task Deve_RetornarListaDeErros_Quando_CorpoDaRequisicaoForMalformado()
    {
        var body = new StringContent("{ \"coverageAmount\": \"não é número\" ", Encoding.UTF8, "application/json");

        var response = await factory.CreateClient().PostAsync("/api/apolices", body);

        var problem = await ShouldBeProblem(response, HttpStatusCode.BadRequest);
        Errors(problem).ShouldContain(error => error.Contains("coverageAmount"));
    }

    [Fact(DisplayName = nameof(Deve_RetornarProblemDetails_Quando_RotaNaoExistir))]
    [Trait("Integration", "Erros - Formato único")]
    public async Task Deve_RetornarProblemDetails_Quando_RotaNaoExistir()
    {
        var response = await factory.CreateClient().GetAsync("/api/rota-inexistente");

        var problem = await ShouldBeProblem(response, HttpStatusCode.NotFound);
        Errors(problem).ShouldNotBeEmpty();
    }

    [Fact(DisplayName = nameof(Deve_RegistrarLogDeErroSemExporDetalhes_Quando_OcorrerErroInesperado))]
    [Trait("Integration", "Erros - Log de 500")]
    public async Task Deve_RegistrarLogDeErroSemExporDetalhes_Quando_OcorrerErroInesperado()
    {
        const string internalDetail = "detalhe interno que não pode vazar";
        var app = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.AddFakeLogging();
            services.AddScoped<IApproveClaimUseCase>(_ => new ThrowingUseCase(new InvalidOperationException(internalDetail)));
        }));

        var response = await app.CreateClient().PostAsync($"/api/sinistros/{Guid.NewGuid()}/aprovacao", null);

        var problem = await ShouldBeProblem(response, HttpStatusCode.InternalServerError);
        Errors(problem).ShouldBe([ResourcesMessageException.UNKNOWN_ERROR]);
        problem.GetRawText().ShouldNotContain(internalDetail);

        var errorLog = app.Services.GetFakeLogCollector().GetSnapshot()
            .Single(record => record.Level == LogLevel.Error && record.Category == "PolicyDesk.Api.Filters.ExceptionFilter");
        errorLog.Exception.ShouldBeOfType<InvalidOperationException>().Message.ShouldBe(internalDetail);
        errorLog.Id.Id.ShouldBe(9001);
    }

    // Garante o formato: content type, status, lista "errors" e traceId igual ao cabeçalho de correlação
    private static async Task<JsonElement> ShouldBeProblem(HttpResponseMessage response, HttpStatusCode expectedStatus)
    {
        response.StatusCode.ShouldBe(expectedStatus);
        response.Content.Headers.ContentType?.MediaType.ShouldBe(ProblemJson);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.EnumerateObject().Count(property => property.Name == ProblemDetailsSetup.ErrorsKey).ShouldBe(1);
        problem.GetProperty("status").GetInt32().ShouldBe((int)expectedStatus);
        problem.GetProperty(ProblemDetailsSetup.ErrorsKey).ValueKind.ShouldBe(JsonValueKind.Array);

        var traceId = problem.GetProperty(ProblemDetailsSetup.TraceIdKey).GetString();
        traceId.ShouldNotBeNullOrWhiteSpace();
        response.Headers.GetValues("X-Trace-Id").Single().ShouldBe(traceId);

        return problem;
    }

    private static List<string> Errors(JsonElement problem) =>
        problem.GetProperty(ProblemDetailsSetup.ErrorsKey).EnumerateArray().Select(error => error.GetString()!).ToList();

    private HttpClient CreateClientThrowing(System.Exception exception) =>
        factory
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
                services.AddScoped<IApproveClaimUseCase>(_ => new ThrowingUseCase(exception))))
            .CreateClient();

    private sealed class ThrowingUseCase(System.Exception exception) : IApproveClaimUseCase
    {
        public Task Execute(Guid claimId, CancellationToken cancellationToken = default) => throw exception;
    }
}
