using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unify.Erp.Domain.Branches;
using Unify.Erp.Domain.Customers;
using Unify.Erp.Domain.Organisations;
using Unify.Erp.Domain.Products;
using Unify.Erp.Domain.Sales;
using Unify.Erp.Domain.Warehouses;

namespace Unify.Erp.Infrastructure.Persistence.Configurations;

public sealed class SalesConfiguration :
    IEntityTypeConfiguration<Sale>,
    IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("sales");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.BranchId).HasColumnName("branch_id").IsRequired();
        builder.Property(entity => entity.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(entity => entity.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(entity => entity.InvoiceNumber).HasColumnName("invoice_number").HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.SaleDateUtc).HasColumnName("sale_date_utc").IsRequired();
        builder.Property(entity => entity.Subtotal).HasColumnName("subtotal").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.DiscountTotal).HasColumnName("discount_total").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.TaxTotal).HasColumnName("tax_total").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.GrandTotal).HasColumnName("grand_total").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status").IsRequired();
        builder.Property(entity => entity.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.InvoiceNumber }).IsUnique();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.CustomerId });
        builder.HasIndex(entity => new { entity.OrganisationId, entity.SaleDateUtc });
        RestrictToOrganisation(builder, "fk_sales_organisations_organisation_id");
        builder.HasOne<Branch>().WithMany().HasForeignKey(entity => entity.BranchId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_sales_branches_branch_id");
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(entity => entity.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_sales_warehouses_warehouse_id");
        builder.HasOne<Customer>().WithMany().HasForeignKey(entity => entity.CustomerId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_sales_customers_customer_id");
    }

    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("sale_items");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasColumnName("id");
        builder.Property(entity => entity.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(entity => entity.SaleId).HasColumnName("sale_id").IsRequired();
        builder.Property(entity => entity.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(entity => entity.Description).HasColumnName("description").HasMaxLength(160).IsRequired();
        builder.Property(entity => entity.Quantity).HasColumnName("quantity").HasPrecision(18, 3).IsRequired();
        builder.Property(entity => entity.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.DiscountAmount).HasColumnName("discount_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.TaxAmount).HasColumnName("tax_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.LineTotal).HasColumnName("line_total").HasPrecision(18, 2).IsRequired();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.SaleId });
        builder.HasOne<Sale>().WithMany().HasForeignKey(entity => entity.SaleId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_sale_items_sales_sale_id");
        builder.HasOne<Product>().WithMany().HasForeignKey(entity => entity.ProductId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_sale_items_products_product_id");
        RestrictToOrganisation(builder, "fk_sale_items_organisations_organisation_id");
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
