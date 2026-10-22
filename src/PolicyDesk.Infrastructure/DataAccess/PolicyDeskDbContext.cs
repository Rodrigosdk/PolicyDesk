using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Infrastructure.DataAccess.Mappings;

[assembly: InternalsVisibleTo("PolicyDesk.Infrastructure.Tests")]
[assembly: InternalsVisibleTo("PolicyDesk.Api.IntegrationTests")]

namespace PolicyDesk.Infrastructure.DataAccess;

internal class PolicyDeskDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Policyholders> Policyholders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PolicyholdersMap());
    }
}