using Microsoft.EntityFrameworkCore;
using Npgsql;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Exception;
using PolicyDesk.Exception.Exception;

namespace PolicyDesk.Infrastructure.DataAccess;

internal class UnitOfWork(PolicyDeskDbContext dbContext) : IUnitOfWork
{
    private const string PolicyholderActiveEmailIndex = "IX_policyholders_email_active";
    private const string UserActiveEmailIndex = "IX_users_email_active";

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Outra operação alterou o registro depois que ele foi lido (xmin diferente)
            throw new ConflictException(ResourcesMessageException.CONCURRENCY_CONFLICT);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
                   {
                       SqlState: PostgresErrorCodes.UniqueViolation,
                       ConstraintName: PolicyholderActiveEmailIndex
                   })
        {
            // Duas operações simultâneas passaram pela verificação de e-mail; o índice barrou a segunda
            throw new ErrorOnValidationException(ResourcesMessageException.POLICYHOLDER_ACTIVE_EMAIL_EXISTS);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
                   {
                       SqlState: PostgresErrorCodes.UniqueViolation,
                       ConstraintName: UserActiveEmailIndex
                   })
        {
            throw new ErrorOnValidationException(ResourcesMessageException.USER_ACTIVE_EMAIL_EXISTS);
        }
    }
}
