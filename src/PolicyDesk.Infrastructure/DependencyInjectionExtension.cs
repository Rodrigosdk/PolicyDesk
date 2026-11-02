using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Claims;
using PolicyDesk.Domain.Repositories.Policies;
using PolicyDesk.Domain.Repositories.Policyholders;
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
            
            services.AddDbContext<PolicyDeskDbContext>(config =>
            {
                var connectionString = configuration.GetConnectionString("DbConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException(
                        "ConnectionStrings:DbConnection não configurada. Informe-a via User Secrets " +
                        "(dotnet user-secrets set \"ConnectionStrings:DbConnection\" \"...\") " +
                        "ou pela variável de ambiente ConnectionStrings__DbConnection.");
                }

                config.UseNpgsql(connectionString);
            });
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}