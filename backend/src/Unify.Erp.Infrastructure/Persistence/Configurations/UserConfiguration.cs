using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unify.Erp.Domain.Users;

namespace Unify.Erp.Infrastructure.Persistence.Configurations;

public sealed class PlatformUserConfiguration : IEntityTypeConfiguration<PlatformUser>
{
    public void Configure(EntityTypeBuilder<PlatformUser> builder)
    {
        builder.ToTable("platform_users");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
        builder.Property(entity => entity.DisplayName).HasColumnName("display_name").HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status").IsRequired();

        builder.Ignore(entity => entity.CanAuthenticate);

        builder.HasIndex(entity => entity.Email).IsUnique();
    }
}

public sealed class UserOrganisationMembershipConfiguration : IEntityTypeConfiguration<UserOrganisationMembership>
{
    public void Configure(EntityTypeBuilder<UserOrganisationMembership> builder)
    {
        builder.ToTable("user_organisation_memberships");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status").IsRequired();

        builder.Ignore(entity => entity.IsActive);

        builder.HasIndex(entity => new { entity.OrganisationId, entity.UserId }).IsUnique();
    }
}
