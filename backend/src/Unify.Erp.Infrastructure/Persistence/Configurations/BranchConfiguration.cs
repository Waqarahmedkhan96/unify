using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unify.Erp.Domain.Branches;

namespace Unify.Erp.Infrastructure.Persistence.Configurations;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("branches");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.Timezone).HasColumnName("timezone").HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status").IsRequired();

        builder.Ignore(entity => entity.IsActive);

        builder.HasIndex(entity => new { entity.OrganisationId, entity.Code }).IsUnique();
    }
}
