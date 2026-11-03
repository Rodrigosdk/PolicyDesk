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
    extension(IServiceCollection services)
    {
        public void AddInfrastructure(IConfiguration configuration)
        {
            services.AddScoped<IPolicyholdersWriteOnlyRepository, PolicyholdersRepository>();
            services.AddScoped<IPolicyholdersReadOnlyRepository, PolicyholdersRepository>();
            services.AddScoped<IPoliciesWriteOnlyRepository, PoliciesRepository>();
            services.AddScoped<IPoliciesReadOnlyRepository, PoliciesRepository>();
            services.AddScoped<IClaimsWriteOnlyRepository, ClaimsRepository>();
            services.AddScoped<IClaimsReadOnlyRepository, ClaimsRepository>();
            
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
}