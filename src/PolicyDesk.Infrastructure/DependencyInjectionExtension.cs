using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Policyholders;
using PolicyDesk.Infrastructure.DataAccess;
using PolicyDesk.Infrastructure.DataAccess.Repositories;

namespace PolicyDesk.Infrastructure;

public static class DependencyInjectionExtension
{
    // Nova forma de extension no .net 10
    extension(IServiceCollection services)
    {
        public void AddInfrastructure(IConfiguration configuration)
        {
            services.AddScoped<IPolicyholdersWriteOnlyRepository, PolicyholdersRepository>();
            services.AddScoped<IPolicyholdersReadOnlyRepository, PolicyholdersRepository>();
            
            services.AddDbContext<PolicyDeskDbContext>(config =>
            {
                var connectionString = configuration.GetConnectionString("DbConnection");
                
                config.UseNpgsql(connectionString);
            });
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}