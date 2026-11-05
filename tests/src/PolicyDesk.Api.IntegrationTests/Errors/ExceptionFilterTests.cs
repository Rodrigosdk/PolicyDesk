using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using PolicyDesk.Api.Filters;
using Shouldly;

namespace PolicyDesk.Api.IntegrationTests.Errors;

public class ExceptionFilterTests(PolicyDeskApplicationFactory factory) : IClassFixture<PolicyDeskApplicationFactory>
{
    [Fact(DisplayName = nameof(Deve_Responder499SemRegistrarErro_Quando_ClienteCancelarARequisicao))]
    [Trait("Integration", "Erros - Cancelamento")]
    public void Deve_Responder499SemRegistrarErro_Quando_ClienteCancelarARequisicao()
    {
        var logger = new FakeLogger<ExceptionFilter>();
        var context = CreateContext(new OperationCanceledException(), new CancellationToken(canceled: true));

        CreateFilter(logger).OnException(context);

        context.Result.ShouldBeOfType<StatusCodeResult>().StatusCode.ShouldBe(ExceptionFilter.StatusClientClosedRequest);
        var record = logger.Collector.GetSnapshot().ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Information);
        record.Id.Id.ShouldBe(9002);
    }

    [Fact(DisplayName = nameof(Deve_TratarComoErroInesperado_Quando_CancelamentoNaoVierDoCliente))]
    [Trait("Integration", "Erros - Cancelamento")]
    public void Deve_TratarComoErroInesperado_Quando_CancelamentoNaoVierDoCliente()
    {
        var logger = new FakeLogger<ExceptionFilter>();
        var context = CreateContext(new OperationCanceledException(), CancellationToken.None);

        CreateFilter(logger).OnException(context);

        context.Result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        logger.Collector.GetSnapshot().ShouldHaveSingleItem().Level.ShouldBe(LogLevel.Error);
    }

    private ExceptionFilter CreateFilter(ILogger<ExceptionFilter> logger) =>
        new(factory.Services.GetRequiredService<ProblemDetailsFactory>(), logger);

    private ExceptionContext CreateContext(System.Exception exception, CancellationToken requestAborted)
    {
        var httpContext = new DefaultHttpContext { RequestAborted = requestAborted, RequestServices = factory.Services };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ExceptionContext(actionContext, []) { Exception = exception };
    }
}
