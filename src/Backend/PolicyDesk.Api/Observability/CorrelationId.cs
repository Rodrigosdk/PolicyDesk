using System.Diagnostics;

namespace PolicyDesk.Api.Observability;

// Identificador de correlação da requisição: o trace id do W3C Trace Context.
// É o mesmo valor presente nos logs e nos traces, então o cliente pode informá-lo ao suporte.
public static class CorrelationId
{
    public const string HeaderName = "X-Trace-Id";

    public static string Get(HttpContext httpContext) =>
        Activity.Current?.TraceId.ToHexString() ?? httpContext.TraceIdentifier;

    public static WebApplication UseCorrelationIdHeader(this WebApplication app)
    {
        app.Use((context, next) =>
        {
            // Gravado no OnStarting: o tratamento de exceções limpa os cabeçalhos antes de escrever o erro
            var traceId = Get(context);
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = traceId;
                return Task.CompletedTask;
            });

            return next(context);
        });

        return app;
    }
}
