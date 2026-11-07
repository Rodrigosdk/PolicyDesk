using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Repositories;
using PolicyDesk.Domain.Repositories.Policyholders;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PolicyDesk.Infrastructure.DataAccess.Mappings;

namespace PolicyDesk.Infrastructure.DataAccess.Repositories;

internal sealed class PolicyholdersRepository(PolicyDeskDbContext dbContext) : 
    IPolicyholdersWriteOnlyRepository,
    IPolicyholdersReadOnlyRepository,
    IPolicyholdersUpdateOnlyRepository
{
    public async Task<PolicyholderRegistrationResult> RegisterOrReactivateAsync(Policyholder policyholder, CancellationToken cancellationToken = default)
    {
        int affectedRows;
        try
        {
            affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO policyholders
                    (id, name, cpf, birth_date, email, phone, created_at, updated_at, active)
                VALUES
                    ({policyholder.Id}, {policyholder.Name}, {policyholder.Cpf}, {DateOnly.FromDateTime(policyholder.BirthDate)},
                     {policyholder.Email}, {policyholder.Phone}, {policyholder.CreatedAt}, {policyholder.UpdatedAt},
                     {policyholder.Active})
                ON CONFLICT (cpf) DO UPDATE
                SET name = EXCLUDED.name,
                    birth_date = EXCLUDED.birth_date,
                    email = EXCLUDED.email,
                    phone = EXCLUDED.phone,
                    active = TRUE,
                    updated_at = CURRENT_TIMESTAMP
                WHERE policyholders.active = FALSE
                """, cancellationToken);
        }
        catch (PostgresException exception) when (
            exception is { 
                SqlState: PostgresErrorCodes.UniqueViolation, 
                ConstraintName: "IX_policyholders_email_active" 
            })
        {
            return PolicyholderRegistrationResult.ActiveEmailAlreadyExists;
        }

        return affectedRows == 0
            ? PolicyholderRegistrationResult.ActiveCpfAlreadyExists
            : PolicyholderRegistrationResult.RegisteredOrReactivated;
    }

    public async Task<PagedResult<Policyholder>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Policyholders.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(policyholder => policyholder.CreatedAt)
            .ThenBy(policyholder => policyholder.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Policyholder>(items, totalCount);
    }

    public Task<Policyholder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Policyholders.AsNoTracking().FirstOrDefaultAsync(policyholder => policyholder.Id == id, cancellationToken);
    }

    public Task<Policyholder?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Policyholders.FirstOrDefaultAsync(policyholder => policyholder.Id == id, cancellationToken);
    }

    // Consulta rastreada de propósito: o cadastro de apólice altera o segurado retornado
    // (RegisterPolicy), e é essa gravação que detecta a inativação concorrente
    public Task<Policyholder?> GetByIdIncludingInactiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Policyholders
            .IgnoreQueryFilters([PolicyholderMap.ActiveFilter])
            .FirstOrDefaultAsync(policyholder => policyholder.Id == id, cancellationToken);
    }
    
    public Task<bool> ExistActivePolicyholdersWithEmailAsync(string email, Guid? exceptPolicyholderId = null, CancellationToken cancellationToken = default)
    {
        return dbContext.Policyholders.AnyAsync(policyholder =>
            policyholder.Email == email &&
            (exceptPolicyholderId == null || policyholder.Id != exceptPolicyholderId), cancellationToken);
    }

    public Task<bool> HasActiveOrFuturePolicyAsync(Guid policyholderId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return dbContext.Policies.AnyAsync(policy =>
            policy.PolicyholderId == policyholderId &&
            policy.Status == PolicyStatus.Active &&
            policy.EndDate >= today, cancellationToken);
    }
}