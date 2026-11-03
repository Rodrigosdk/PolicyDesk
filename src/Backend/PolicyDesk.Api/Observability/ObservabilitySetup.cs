using Microsoft.AspNetCore.HttpLogging;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace PolicyDesk.Api.Observability;

public static class ObservabilitySetup
{
    // Variável padrão do OpenTelemetry; sem ela, nada é exportado (testes e execução local sem coletor)
    private const string OtlpEndpointKey = "OTEL_EXPORTER_OTLP_ENDPOINT";

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        // Uma linha por requisição (método, rota, status e duração). Sem corpo, query string ou
        // cabeçalhos, porque podem conter CPF, e-mail e telefone (LGPD).
        builder.Services.AddHttpLogging(options =>
        {
            options.LoggingFields = HttpLoggingFields.RequestMethod
                                    | HttpLoggingFields.RequestPath
                                    | HttpLoggingFields.ResponseStatusCode
                                    | HttpLoggingFields.Duration;
            options.CombineLogs = true;
        });

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
        });

        var openTelemetry = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                .AddHttpClientInstrumentation()
                .AddNpgsql())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("Microsoft.EntityFrameworkCore")
                .AddMeter("Npgsql"));

        if (!string.IsNullOrWhiteSpace(builder.Configuration[OtlpEndpointKey]))
            openTelemetry.UseOtlpExporter();

        return builder;
    }
}
