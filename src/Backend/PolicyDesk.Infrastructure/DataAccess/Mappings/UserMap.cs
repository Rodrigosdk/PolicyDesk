using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Validations;

namespace PolicyDesk.Infrastructure.DataAccess.Mappings;

internal class UserMap : IEntityTypeConfiguration<User>
{
    public const string ActiveFilter = "ActiveUsers";

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.HasQueryFilter(ActiveFilter, user => user.Active);

        builder.Property(user => user.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(user => user.Name)
            .HasColumnName("name")
            .HasMaxLength(FieldLimits.UserName)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasColumnName("email")
            .HasMaxLength(FieldLimits.Email)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .HasDatabaseName("IX_users_email_active")
            .IsUnique()
            .HasFilter("\"active\" = TRUE");

        builder.Property(user => user.Password)
            .HasColumnName("password")
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(user => user.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(user => user.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.Property<uint>("Version").IsRowVersion();
    }
}
