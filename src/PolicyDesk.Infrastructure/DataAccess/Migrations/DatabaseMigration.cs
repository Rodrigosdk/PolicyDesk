using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace PolicyDesk.Infrastructure.DataAccess.Migrations;

public static class DatabaseMigration
{
    public static async Task ExecuteMigrationsAsync(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<PolicyDeskDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}