using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unify.Erp.Domain.Organisations;

namespace Unify.Erp.Infrastructure.Persistence.Configurations;

public sealed class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder.ToTable("organisations");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.LegalName).HasColumnName("legal_name").HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.DisplayName).HasColumnName("display_name").HasMaxLength(120).IsRequired();
        builder.Property(entity => entity.BaseCurrency).HasColumnName("base_currency").HasMaxLength(3).IsRequired();
        builder.Property(entity => entity.Timezone).HasColumnName("timezone").HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status").IsRequired();
        builder.Property(entity => entity.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.Ignore(entity => entity.IsActive);

        builder.HasIndex(entity => entity.DisplayName);
    }
}
