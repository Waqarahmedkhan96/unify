using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unify.Erp.Infrastructure.Auth;

namespace Unify.Erp.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshTokenRecord>
{
    public void Configure(EntityTypeBuilder<RefreshTokenRecord> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id");
        builder.Property(entity => entity.DeviceId).HasColumnName("device_id");
        builder.Property(entity => entity.FamilyId).HasColumnName("family_id").IsRequired();
        builder.Property(entity => entity.TokenHash).HasColumnName("token_hash").HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(entity => entity.ExpiresAtUtc).HasColumnName("expires_at_utc").IsRequired();
        builder.Property(entity => entity.RevokedAtUtc).HasColumnName("revoked_at_utc");
        builder.Property(entity => entity.ReplacedByTokenId).HasColumnName("replaced_by_token_id");

        builder.HasIndex(entity => entity.TokenHash).IsUnique();
        builder.HasIndex(entity => new { entity.UserId, entity.FamilyId });
    }
}
