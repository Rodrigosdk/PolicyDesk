using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Validations;

namespace PolicyDesk.Infrastructure.DataAccess.Mappings;

internal class PolicyholderMap : IEntityTypeConfiguration<Policyholder>
{
    public const string ActiveFilter = "ActivePolicyholders";

    public void Configure(EntityTypeBuilder<Policyholder> builder)
    {
        builder.ToTable("policyholders");

        builder.HasKey(policyholder => policyholder.Id);

        builder.HasQueryFilter(ActiveFilter, policyholder => policyholder.Active);

        builder.Property(policyholder => policyholder.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(policyholder => policyholder.Name)
            .HasColumnName("name")
            .HasMaxLength(FieldLimits.PolicyholderName)
            .IsRequired();

        builder.Property(policyholder => policyholder.Cpf)
            .HasColumnName("cpf")
            .HasMaxLength(FieldLimits.Cpf)
            .IsRequired();

        builder.HasIndex(policyholder => policyholder.Cpf)
            .IsUnique();

        builder.Property(policyholder => policyholder.BirthDate)
            .HasColumnName("birth_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(policyholder => policyholder.Email)
            .HasColumnName("email")
            .HasMaxLength(FieldLimits.Email)
            .IsRequired();

        builder.HasIndex(policyholder => policyholder.Email)
            .HasDatabaseName("IX_policyholders_email_active")
            .IsUnique()
            .HasFilter("\"active\" = TRUE");

        builder.Property(policyholder => policyholder.Phone)
            .HasColumnName("phone")
            .HasMaxLength(FieldLimits.Phone)
            .IsRequired();

        builder.Property(policyholder => policyholder.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(policyholder => policyholder.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(policyholder => policyholder.Active)
            .HasColumnName("active")
            .IsRequired();

        // Concorrência otimista: o Npgsql mapeia este uint para a coluna de sistema xmin,
        // que o PostgreSQL altera a cada UPDATE. Gravar com um xmin desatualizado falha.
        builder.Property<uint>("Version").IsRowVersion();
    }
}