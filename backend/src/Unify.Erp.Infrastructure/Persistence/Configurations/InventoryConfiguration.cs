using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unify.Erp.Domain.Inventory;
using Unify.Erp.Domain.Organisations;
using Unify.Erp.Domain.Products;
using Unify.Erp.Domain.Warehouses;

namespace Unify.Erp.Infrastructure.Persistence.Configurations;

public sealed class InventoryConfiguration :
    IEntityTypeConfiguration<StockMovement>,
    IEntityTypeConfiguration<StockBalance>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.BranchId).HasColumnName("branch_id").IsRequired();
        builder.Property(entity => entity.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(entity => entity.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(entity => entity.MovementType).HasColumnName("movement_type").IsRequired();
        builder.Property(entity => entity.Quantity).HasColumnName("quantity").HasPrecision(18, 3).IsRequired();
        builder.Property(entity => entity.ReferenceType).HasColumnName("reference_type").HasMaxLength(60).IsRequired();
        builder.Property(entity => entity.ReferenceId).HasColumnName("reference_id");
        builder.Property(entity => entity.Notes).HasColumnName("notes").HasMaxLength(500);
        builder.Property(entity => entity.OccurredAtUtc).HasColumnName("occurred_at_utc").IsRequired();
        builder.Property(entity => entity.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Ignore(entity => entity.SignedQuantity);
        builder.HasIndex(entity => new { entity.OrganisationId, entity.WarehouseId, entity.ProductId });
        builder.HasIndex(entity => new { entity.OrganisationId, entity.OccurredAtUtc });
        RestrictToOrganisation(builder, "fk_stock_movements_organisations_organisation_id");
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(entity => entity.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_stock_movements_warehouses_warehouse_id");
        builder.HasOne<Product>().WithMany().HasForeignKey(entity => entity.ProductId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_stock_movements_products_product_id");
    }

    public void Configure(EntityTypeBuilder<StockBalance> builder)
    {
        builder.ToTable("stock_balances");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(entity => entity.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(entity => entity.QuantityOnHand).HasColumnName("quantity_on_hand").HasPrecision(18, 3).IsRequired();
        builder.Property(entity => entity.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.WarehouseId, entity.ProductId }).IsUnique();
        RestrictToOrganisation(builder, "fk_stock_balances_organisations_organisation_id");
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(entity => entity.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_stock_balances_warehouses_warehouse_id");
        builder.HasOne<Product>().WithMany().HasForeignKey(entity => entity.ProductId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_stock_balances_products_product_id");
    }

    private static void RestrictToOrganisation<TEntity>(EntityTypeBuilder<TEntity> builder, string constraintName)
        where TEntity : class
    {
        builder.HasOne<Organisation>()
            .WithMany()
            .HasForeignKey("OrganisationId")
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName(constraintName);
    }
}
