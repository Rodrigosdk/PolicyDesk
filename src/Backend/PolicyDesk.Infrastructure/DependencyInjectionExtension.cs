using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Claims;
using PolicyDesk.Domain.Repositories.Policies;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Domain.SeedWorks;
using PolicyDesk.Exception;
using PolicyDesk.Infrastructure.Configuration;
using PolicyDesk.Infrastructure.DataAccess;
using PolicyDesk.Infrastructure.DataAccess.Repositories;

namespace PolicyDesk.Infrastructure;

public static class DependencyInjectionExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddTimer(services, configuration);
        AddDatabase(services, configuration);
        AddRepositories(services);
    }

    private static void AddRepositories(IServiceCollection services)
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

    }
    
    private static void AddDatabase(IServiceCollection services,  IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.DbConnection),
                ResourcesMessageException.MISSING_CONNECTION_STRING)
            .ValidateOnStart();

        services.AddDbContext<PolicyDeskDbContext>((serviceProvider, config) =>
            config.UseNpgsql(serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value.DbConnection));
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHealthChecks().AddDbContextCheck<PolicyDeskDbContext>("database");
    }
    
    private static void AddTimer(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BusinessOptions>()
            .Bind(configuration.GetSection(BusinessOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.TimeZone) && TimeZoneInfo.TryFindSystemTimeZoneById(options.TimeZone, out _),
                ResourcesMessageException.INVALID_BUSINESS_TIME_ZONE)
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(serviceProvider => new BusinessClock(
            serviceProvider.GetRequiredService<TimeProvider>(),
            TimeZoneInfo.FindSystemTimeZoneById(serviceProvider.GetRequiredService<IOptions<BusinessOptions>>().Value.TimeZone)));

    }

}
