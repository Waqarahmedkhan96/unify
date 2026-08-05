using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unify.Erp.Domain.Sessions;

namespace Unify.Erp.Infrastructure.Persistence.Configurations;

public sealed class DeviceSessionConfiguration : IEntityTypeConfiguration<DeviceSession>
{
    public void Configure(EntityTypeBuilder<DeviceSession> builder)
    {
        builder.ToTable("device_sessions");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(entity => entity.DeviceId).HasColumnName("device_id").IsRequired();
        builder.Property(entity => entity.ExpiresAtUtc).HasColumnName("expires_at_utc").IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status").IsRequired();

        builder.HasIndex(entity => new { entity.OrganisationId, entity.UserId });
        builder.HasIndex(entity => new { entity.OrganisationId, entity.DeviceId });
    }
}
