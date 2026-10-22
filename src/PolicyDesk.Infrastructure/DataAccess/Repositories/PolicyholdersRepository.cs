using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Enums;
using PolicyDesk.Domain.Repositories.Policyholders;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace PolicyDesk.Infrastructure.DataAccess.Repositories;

internal sealed class PolicyholdersRepository(PolicyDeskDbContext dbContext) : 
    IPolicyholdersWriteOnlyRepository,
    IPolicyholdersReadOnlyRepository
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
            .Where(policyholder => policyholder.Active)
            .ToListAsync();
    }

    public Task<Policyholders?> GetById(Guid id)
    {
        return dbContext.Policyholders.FirstOrDefaultAsync(policyholder =>
            policyholder.Id == id && policyholder.Active);
    }
    
    public Task<bool> ExistActivePolicyholdersWithEmail(string email, Guid? exceptPolicyholderId = null)
    {
        return dbContext.Policyholders.AnyAsync(policyholder =>
            policyholder.Email == email &&
            policyholder.Active &&
            (exceptPolicyholderId == null || policyholder.Id != exceptPolicyholderId));
    }

    public Task<bool> HasActivePolicy(Guid policyholderId)
    {
        throw new NotImplementedException();
    }
}