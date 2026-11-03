using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PolicyDesk.Infrastructure.DataAccess;

// Usada só pelas ferramentas do EF (dotnet ef migrations add / script), nunca pela API.
// Gerar migrations e scripts não conecta ao banco, então uma connection string de referência basta;
// para comandos que conectam (database update), defina ConnectionStrings__DbConnection.
internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PolicyDeskDbContext>
{
    private const string ReferenceConnectionString = "Host=localhost;Port=5432;Database=PolicyDesk";

    public PolicyDeskDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DbConnection");

        var options = new DbContextOptionsBuilder<PolicyDeskDbContext>()
            .UseNpgsql(string.IsNullOrWhiteSpace(connectionString) ? ReferenceConnectionString : connectionString)
            .Options;

        return new PolicyDeskDbContext(options);
    }
}
