using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolicyDesk.Domain.Entities;

namespace PolicyDesk.Infrastructure.DataAccess.Mappings;

internal class PolicyholdersMap : IEntityTypeConfiguration<Policyholders>
{
    public void Configure(EntityTypeBuilder<Policyholders> builder)
    {
        builder.ToTable("policyholders");

        builder.HasKey(policyholder => policyholder.Id);

        builder.Property(policyholder => policyholder.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(policyholder => policyholder.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(policyholder => policyholder.Cpf)
            .HasColumnName("cpf")
            .HasMaxLength(11)
            .IsRequired();

        builder.HasIndex(policyholder => policyholder.Cpf)
            .IsUnique();

        builder.Property(policyholder => policyholder.BirthDate)
            .HasColumnName("birth_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(policyholder => policyholder.Email)
            .HasColumnName("email")
            .HasMaxLength(254)
            .IsRequired();

        builder.HasIndex(policyholder => policyholder.Email)
            .HasDatabaseName("IX_policyholders_email_active")
            .IsUnique()
            .HasFilter("\"active\" = TRUE");

        builder.Property(policyholder => policyholder.Phone)
            .HasColumnName("phone")
            .IsRequired();

        builder.Property(policyholder => policyholder.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(policyholder => policyholder.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(policyholder => policyholder.Active)
            .HasColumnName("active")
            .IsRequired();
    }
}