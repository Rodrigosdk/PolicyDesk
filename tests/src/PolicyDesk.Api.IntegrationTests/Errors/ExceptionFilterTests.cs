using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using PolicyDesk.Application.Claims.Approve;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Errors;

public class ExceptionFilterTests(PolicyDeskApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact(DisplayName = nameof(Deve_RetornarBadRequestComMensagem_Quando_RegraDeDominioForViolada))]
    [Trait("Integration", "Erros - Regra de domínio")]
    public async Task Deve_RetornarBadRequestComMensagem_Quando_RegraDeDominioForViolada()
    {
        // Simula uma regra do domínio que escapou sem validação prévia no caso de uso
        var client = factory
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
                services.AddScoped<IApproveClaimUseCase, DomainRuleViolatingUseCase>()))
            .CreateClient();

        var response = await client.PostAsync($"/api/sinistros/{Guid.NewGuid()}/aprovacao", null);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain(ResourcesMessageException.INVALID_PHONE);
    }

    private sealed class DomainRuleViolatingUseCase : IApproveClaimUseCase
    {
        public Task Execute(Guid claimId) =>
            throw new DomainRuleException(ResourcesMessageException.INVALID_PHONE);
    }
}
