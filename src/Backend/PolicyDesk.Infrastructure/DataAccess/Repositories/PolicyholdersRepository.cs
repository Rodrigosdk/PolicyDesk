using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
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
    public async Task<PolicyholderRegistrationResult> RegisterOrReactivate(Policyholders policyholder)
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
                """);
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

    public async Task<IEnumerable<Policyholders>> GetAll()
    {
        return await dbContext.Policyholders
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<Policyholders?> GetById(Guid id)
    {
        return dbContext.Policyholders.AsNoTracking().FirstOrDefaultAsync(policyholder => policyholder.Id == id);
    }

    public Task<Policyholders?> GetByIdForUpdate(Guid id)
    {
        return dbContext.Policyholders.FirstOrDefaultAsync(policyholder => policyholder.Id == id);
    }

    // Consulta rastreada de propósito: o cadastro de apólice altera o segurado retornado
    // (RegisterPolicy), e é essa gravação que detecta a inativação concorrente
    public Task<Policyholders?> GetByIdIncludingInactive(Guid id)
    {
        return dbContext.Policyholders
            .IgnoreQueryFilters([PolicyholdersMap.ActiveFilter])
            .FirstOrDefaultAsync(policyholder => policyholder.Id == id);
    }
    
    public Task<bool> ExistActivePolicyholdersWithEmail(string email, Guid? exceptPolicyholderId = null)
    {
        return dbContext.Policyholders.AnyAsync(policyholder =>
            policyholder.Email == email &&
            (exceptPolicyholderId == null || policyholder.Id != exceptPolicyholderId));
    }

    public Task<bool> HasActiveOrFuturePolicy(Guid policyholderId)
    {
        var today = DateTime.UtcNow.Date;
        return dbContext.Policies.AnyAsync(policy =>
            policy.PolicyholderId == policyholderId &&
            policy.Status == PolicyStatus.Active &&
            policy.EndDate >= today);
    }
}