using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unify.Erp.Domain.Organisations;
using Unify.Erp.Domain.Products;

namespace Unify.Erp.Infrastructure.Persistence.Configurations;

public sealed class ProductCatalogConfiguration :
    IEntityTypeConfiguration<UnitOfMeasure>,
    IEntityTypeConfiguration<ProductCategory>,
    IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("units_of_measure");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.Code).HasColumnName("code").HasMaxLength(16).IsRequired();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        builder.Property(entity => entity.DecimalPlaces).HasColumnName("decimal_places").IsRequired();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.Code }).IsUnique();
        RestrictToOrganisation(builder, "fk_units_of_measure_organisations_organisation_id");
    }

    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("product_categories");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.Code).HasColumnName("code").HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.Code }).IsUnique();
        RestrictToOrganisation(builder, "fk_product_categories_organisations_organisation_id");
    }

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.UnitOfMeasureId).HasColumnName("unit_of_measure_id").IsRequired();
        builder.Property(entity => entity.CategoryId).HasColumnName("category_id");
        builder.Property(entity => entity.ProductCode).HasColumnName("product_code").HasMaxLength(32).IsRequired();
        builder.Property(entity => entity.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Barcode).HasColumnName("barcode").HasMaxLength(80);
        builder.Property(entity => entity.PurchasePrice).HasColumnName("purchase_price").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.SalesPrice).HasColumnName("sales_price").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.IsInventoryTracked).HasColumnName("is_inventory_tracked").IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status").IsRequired();
        builder.Property(entity => entity.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Ignore(entity => entity.IsActive);
        builder.HasIndex(entity => new { entity.OrganisationId, entity.ProductCode }).IsUnique();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.CategoryId });
        builder.HasIndex(entity => new { entity.OrganisationId, entity.Name });
        RestrictToOrganisation(builder, "fk_products_organisations_organisation_id");
        builder.HasOne<UnitOfMeasure>().WithMany().HasForeignKey(entity => entity.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_products_units_of_measure_unit_of_measure_id");
        builder.HasOne<ProductCategory>().WithMany().HasForeignKey(entity => entity.CategoryId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_products_product_categories_category_id");
    }

    private static void RestrictToOrganisation<TEntity>(
        EntityTypeBuilder<TEntity> builder,
        string constraintName)
        where TEntity : class
    {
        builder.HasOne<Organisation>()
            .WithMany()
            .HasForeignKey("OrganisationId")
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName(constraintName);
    }
}
