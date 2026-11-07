using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolicyDesk.Domain.Entities;
using PolicyDesk.Domain.Validations;

namespace PolicyDesk.Infrastructure.DataAccess.Mappings;

internal class ClaimMap : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("claims");

        builder.HasKey(claim => claim.Id);
        builder.Property(claim => claim.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(claim => claim.PolicyId).HasColumnName("policy_id").IsRequired();
        builder.Property(claim => claim.Description).HasColumnName("description").HasMaxLength(FieldLimits.Description).IsRequired();
        builder.Property(claim => claim.Amount).HasColumnName("amount").HasPrecision(MoneyLimits.Precision, MoneyLimits.Scale).IsRequired();
        builder.Property(claim => claim.OccurredOn).HasColumnName("occurred_on").HasColumnType("date").IsRequired();
        builder.Property(claim => claim.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(claim => claim.ApprovedAt).HasColumnName("approved_at");
        builder.Property(claim => claim.DenialReason).HasColumnName("denial_reason").HasMaxLength(FieldLimits.DenialReason);
        builder.Property(claim => claim.PaidAt).HasColumnName("paid_at");
        builder.Property(claim => claim.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(claim => claim.UpdatedAt).HasColumnName("updated_at");

        // Concorrência otimista: o Npgsql mapeia este uint para a coluna de sistema xmin,
        // que o PostgreSQL altera a cada UPDATE. Gravar com um xmin desatualizado falha.
        builder.Property<uint>("Version").IsRowVersion();

        builder.HasOne<Policy>()
            .WithMany()
            .HasForeignKey(claim => claim.PolicyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
