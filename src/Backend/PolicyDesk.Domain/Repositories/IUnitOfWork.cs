namespace PolicyDesk.Domain.Repositories;

public interface IUnitOfWork
{
    Task Commit();
}