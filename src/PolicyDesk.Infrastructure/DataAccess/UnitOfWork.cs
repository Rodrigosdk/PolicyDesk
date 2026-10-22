using PolicyDesk.Domain.Repositories;

namespace PolicyDesk.Infrastructure.DataAccess;

internal class UnitOfWork(PolicyDeskDbContext dbContext) : IUnitOfWork
{
    public async Task Commit()
    {
        await dbContext.SaveChangesAsync();
    }
}