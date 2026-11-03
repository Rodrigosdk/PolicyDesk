using System.Globalization;
using Microsoft.AspNetCore.Localization;
using PolicyDesk.Api.Errors;
using PolicyDesk.Api.Filters;
using PolicyDesk.Api.Observability;
using PolicyDesk.Application;
using PolicyDesk.Infrastructure;
using PolicyDesk.Api.Converters;
using PolicyDesk.Infrastructure.DataAccess.Migrations;

var builder = WebApplication.CreateBuilder(args);

const long maxRequestBodySizeInBytes = 64 * 1024;
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = maxRequestBodySizeInBytes);

builder.AddObservability();
builder.Services.AddErrorHandling();

builder.Services.AddControllers(options => options.Filters.Add<ExceptionFilter>()).AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new StringConverter()));
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new List<CultureInfo>
    {
        new CultureInfo("en"),
        new CultureInfo("pt-BR")
    };
    
    options.DefaultRequestCulture = new RequestCulture("en");
    
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];
});

builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

// Ordem: o cabeçalho de correlação e o log de requisições envolvem o tratamento de erros,
// para que o status final (inclusive 500) apareça no log e o X-Trace-Id vá em toda resposta
app.UseCorrelationIdHeader();
app.UseHttpLogging();
app.UseErrorHandling();

app.UseRequestLocalization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "PolicyDesk API"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

if (!app.Environment.IsProduction())
{
    await ExecuteMigrationsAsync();
}

app.Run();

async Task ExecuteMigrationsAsync()
{
    await using var scope = app.Services.CreateAsyncScope();
    
    await DatabaseMigration.ExecuteMigrationsAsync(scope.ServiceProvider);
}

public partial class Program { }