using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Validations;

namespace PolicyDesk.Infrastructure.DataAccess.Mappings;

internal class PolicyMap : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("policies");

        builder.HasKey(policy => policy.Id);
        builder.Property(policy => policy.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();
        builder.Property(policy => policy.PolicyholderId)
            .HasColumnName("policyholder_id")
            .IsRequired();
        builder.Property(policy => policy.Description)
            .HasColumnName("description")
            .HasMaxLength(FieldLimits.Description)
            .IsRequired();
        builder.Property(policy => policy.CoverageAmount)
            .HasColumnName("coverage_amount")
            .HasPrecision(MoneyLimits.Precision, MoneyLimits.Scale)
            .IsRequired();
        builder.Property(policy => policy.PremiumAmount)
            .HasColumnName("premium_amount")
            .HasPrecision(MoneyLimits.Precision, MoneyLimits.Scale)
            .IsRequired();
        builder.Property(policy => policy.StartDate)
            .HasColumnName("start_date")
            .HasColumnType("date")
            .IsRequired();
        builder.Property(policy => policy.EndDate)
            .HasColumnName("end_date")
            .HasColumnType("date")
            .IsRequired();
        builder.Property(policy => policy.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(FieldLimits.Status)
            .IsRequired();
        builder.Property(policy => policy.CancelledAt)
            .HasColumnName("cancelled_at");
        builder.Property(policy => policy.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
        builder.Property(policy => policy.UpdatedAt)
            .HasColumnName("updated_at");

        // Concorrência otimista: o Npgsql mapeia este uint para a coluna de sistema xmin,
        // que o PostgreSQL altera a cada UPDATE. Gravar com um xmin desatualizado falha.
        builder.Property<uint>("Version").IsRowVersion();

        builder.HasOne<Policyholder>()
            .WithMany()
            .HasForeignKey(policy => policy.PolicyholderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
