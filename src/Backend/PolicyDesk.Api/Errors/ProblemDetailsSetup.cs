using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PolicyDesk.Api.Observability;
using PolicyDesk.Exception;

namespace PolicyDesk.Api.Errors;

// Formato único de erro da API: ProblemDetails (RFC 9457) com a lista "errors" e o "traceId" da requisição
public static class ProblemDetailsSetup
{
    public const string ErrorsKey = "errors";
    public const string TraceIdKey = "traceId";

    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            var problem = context.ProblemDetails;

            problem.Instance ??= context.HttpContext.Request.Path;
            problem.Extensions[TraceIdKey] = CorrelationId.Get(context.HttpContext);

            // Erros gerados pelo próprio ASP.NET (404 de rota, 405, 415...) também trazem a lista "errors".
            // ValidationProblemDetails já serializa a própria propriedade "errors" e não pode recebê-la de novo.
            if (problem is not HttpValidationProblemDetails && !problem.Extensions.ContainsKey(ErrorsKey))
                problem.Extensions[ErrorsKey] = new List<string> { problem.Detail ?? problem.Title ?? ResourcesMessageException.UNKNOWN_ERROR };
        });

        // Corpo malformado ou campo com tipo errado: o [ApiController] devolveria ValidationProblemDetails,
        // com "errors" em formato de dicionário. Aqui ele passa a usar a mesma lista das demais respostas.
        // PostConfigure: o AddControllers() registra a fábrica padrão e sobrescreveria um Configure comum.
        services.PostConfigure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(entry => entry.Errors)
                .Select(error => error.ErrorMessage)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct()
                .ToList();

            if (errors.Count == 0)
                errors.Add(ResourcesMessageException.INVALID_REQUEST);

            var problem = context.HttpContext.RequestServices
                .GetRequiredService<ProblemDetailsFactory>()
                .CreateProblemDetails(context.HttpContext, StatusCodes.Status400BadRequest);
            problem.Extensions[ErrorsKey] = errors;

            return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
        });

        return services;
    }

    public static WebApplication UseErrorHandling(this WebApplication app)
    {
        // Rede de segurança para exceções fora dos controllers (middlewares). As dos controllers
        // são tratadas pelo ExceptionFilter. O handler padrão registra o erro e responde 500 em ProblemDetails.
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        return app;
    }
}
