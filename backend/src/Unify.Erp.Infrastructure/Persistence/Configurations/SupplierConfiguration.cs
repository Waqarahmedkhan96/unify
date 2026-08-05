using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unify.Erp.Domain.Organisations;
using Unify.Erp.Domain.Suppliers;

namespace Unify.Erp.Infrastructure.Persistence.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");

        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.SupplierNumber).HasColumnName("supplier_number").HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.DisplayName).HasColumnName("display_name").HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.LegalName).HasColumnName("legal_name").HasMaxLength(200);
        builder.Property(entity => entity.Phone).HasColumnName("phone").HasMaxLength(40);
        builder.Property(entity => entity.Email).HasColumnName("email").HasMaxLength(254);
        builder.Property(entity => entity.TaxNumber).HasColumnName("tax_number").HasMaxLength(80);
        builder.Property(entity => entity.Status).HasColumnName("status").IsRequired();
        builder.Property(entity => entity.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.Ignore(entity => entity.IsActive);

        builder.HasIndex(entity => new { entity.OrganisationId, entity.SupplierNumber }).IsUnique();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.DisplayName });

        builder.HasOne<Organisation>()
            .WithMany()
            .HasForeignKey(entity => entity.OrganisationId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_suppliers_organisations_organisation_id");
    }
}
