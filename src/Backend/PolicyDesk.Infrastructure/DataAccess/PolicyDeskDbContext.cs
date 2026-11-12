using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Infrastructure.DataAccess.Mappings;

[assembly: InternalsVisibleTo("PolicyDesk.Infrastructure.Tests")]
[assembly: InternalsVisibleTo("PolicyDesk.Api.IntegrationTests")]
[assembly: InternalsVisibleTo("CommonTestUtilities")]

namespace PolicyDesk.Infrastructure.DataAccess;

internal class PolicyDeskDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Policyholder> Policyholders { get; set; }
    public DbSet<Policy> Policies { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PolicyholderMap());
        modelBuilder.ApplyConfiguration(new PolicyMap());
        modelBuilder.ApplyConfiguration(new ClaimMap());
        modelBuilder.ApplyConfiguration(new UserMap());
    }
}