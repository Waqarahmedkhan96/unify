using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unify.Erp.Domain.Access;

namespace Unify.Erp.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.Key).HasColumnName("key").HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Description).HasColumnName("description").HasMaxLength(240).IsRequired();

        builder.HasIndex(entity => entity.Key).IsUnique();
    }
}

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles_catalog");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status").IsRequired();

        builder.Ignore(entity => entity.PermissionKeys);

        builder.HasIndex(entity => new { entity.OrganisationId, entity.Name }).IsUnique();
    }
}
