using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unify.Erp.Domain.Audit;

namespace Unify.Erp.Infrastructure.Persistence.Configurations;

public sealed class AuditConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_entries");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id");
        builder.Property(entity => entity.UserId).HasColumnName("user_id");
        builder.Property(entity => entity.UserEmail).HasColumnName("user_email").HasMaxLength(254);
        builder.Property(entity => entity.CorrelationId).HasColumnName("correlation_id").HasMaxLength(128);
        builder.Property(entity => entity.EntityName).HasColumnName("entity_name").HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.EntityId).HasColumnName("entity_id").HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.Operation).HasColumnName("operation").IsRequired();
        builder.Property(entity => entity.ChangedProperties).HasColumnName("changed_properties").HasColumnType("jsonb");
        builder.Property(entity => entity.OldValues).HasColumnName("old_values").HasColumnType("jsonb");
        builder.Property(entity => entity.NewValues).HasColumnName("new_values").HasColumnType("jsonb");
        builder.Property(entity => entity.OccurredAtUtc).HasColumnName("occurred_at_utc").IsRequired();

        builder.HasIndex(entity => new { entity.OrganisationId, entity.OccurredAtUtc });
        builder.HasIndex(entity => new { entity.EntityName, entity.EntityId });
        builder.HasIndex(entity => entity.CorrelationId);
    }
}
