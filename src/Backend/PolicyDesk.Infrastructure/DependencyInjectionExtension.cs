using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Claims;
using PolicyDesk.Domain.Repositories.Policies;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Infrastructure.Configuration;
using PolicyDesk.Infrastructure.DataAccess;
using PolicyDesk.Infrastructure.DataAccess.Repositories;

namespace PolicyDesk.Infrastructure;

public static class DependencyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPolicyholdersWriteOnlyRepository, PolicyholdersRepository>();
        services.AddScoped<IPolicyholdersReadOnlyRepository, PolicyholdersRepository>();
        services.AddScoped<IPolicyholdersUpdateOnlyRepository, PolicyholdersRepository>();
        services.AddScoped<IPoliciesWriteOnlyRepository, PoliciesRepository>();
        services.AddScoped<IPoliciesReadOnlyRepository, PoliciesRepository>();
        services.AddScoped<IPoliciesUpdateOnlyRepository, PoliciesRepository>();
        services.AddScoped<IClaimsWriteOnlyRepository, ClaimsRepository>();
        services.AddScoped<IClaimsReadOnlyRepository, ClaimsRepository>();
        services.AddScoped<IClaimsUpdateOnlyRepository, ClaimsRepository>();
        
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.DbConnection),
                DatabaseOptions.MissingConnectionStringMessage)
            .ValidateOnStart();

        services.AddDbContext<PolicyDeskDbContext>((serviceProvider, config) =>
            config.UseNpgsql(serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value.DbConnection));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHealthChecks().AddDbContextCheck<PolicyDeskDbContext>("database");
    }
}