using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unify.Erp.Domain.Branches;
using Unify.Erp.Domain.Organisations;
using Unify.Erp.Domain.Products;
using Unify.Erp.Domain.Purchasing;
using Unify.Erp.Domain.Suppliers;
using Unify.Erp.Domain.Warehouses;

namespace Unify.Erp.Infrastructure.Persistence.Configurations;

public sealed class PurchasingConfiguration :
    IEntityTypeConfiguration<PurchaseOrder>,
    IEntityTypeConfiguration<PurchaseOrderItem>,
    IEntityTypeConfiguration<GoodsReceipt>,
    IEntityTypeConfiguration<GoodsReceiptItem>,
    IEntityTypeConfiguration<SupplierInvoice>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("purchase_orders");
        ConfigureDocumentBase(builder);
        builder.Property(entity => entity.SupplierId).HasColumnName("supplier_id").IsRequired();
        builder.Property(entity => entity.OrderNumber).HasColumnName("order_number").HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.OrderDateUtc).HasColumnName("order_date_utc").IsRequired();
        builder.Property(entity => entity.Subtotal).HasColumnName("subtotal").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.TaxTotal).HasColumnName("tax_total").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.GrandTotal).HasColumnName("grand_total").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status").IsRequired();
        builder.Property(entity => entity.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.OrderNumber }).IsUnique();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.SupplierId });
        RestrictSupplier(builder, "fk_purchase_orders_suppliers_supplier_id");
    }

    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("purchase_order_items");
        ConfigureLineBase(builder);
        builder.Property(entity => entity.PurchaseOrderId).HasColumnName("purchase_order_id").IsRequired();
        builder.Property(entity => entity.UnitCost).HasColumnName("unit_cost").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.TaxAmount).HasColumnName("tax_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.LineTotal).HasColumnName("line_total").HasPrecision(18, 2).IsRequired();
        builder.HasOne<PurchaseOrder>().WithMany().HasForeignKey(entity => entity.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_purchase_order_items_purchase_orders_purchase_order_id");
    }

    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("goods_receipts");
        ConfigureDocumentBase(builder);
        builder.Property(entity => entity.WarehouseId).HasColumnName("warehouse_id").IsRequired();
        builder.Property(entity => entity.SupplierId).HasColumnName("supplier_id").IsRequired();
        builder.Property(entity => entity.PurchaseOrderId).HasColumnName("purchase_order_id");
        builder.Property(entity => entity.ReceiptNumber).HasColumnName("receipt_number").HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.ReceiptDateUtc).HasColumnName("receipt_date_utc").IsRequired();
        builder.Property(entity => entity.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.ReceiptNumber }).IsUnique();
        RestrictSupplier(builder, "fk_goods_receipts_suppliers_supplier_id");
        builder.HasOne<Warehouse>().WithMany().HasForeignKey(entity => entity.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_goods_receipts_warehouses_warehouse_id");
        builder.HasOne<PurchaseOrder>().WithMany().HasForeignKey(entity => entity.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_goods_receipts_purchase_orders_purchase_order_id");
    }

    public void Configure(EntityTypeBuilder<GoodsReceiptItem> builder)
    {
        builder.ToTable("goods_receipt_items");
        ConfigureLineBase(builder);
        builder.Property(entity => entity.GoodsReceiptId).HasColumnName("goods_receipt_id").IsRequired();
        builder.HasOne<GoodsReceipt>().WithMany().HasForeignKey(entity => entity.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_goods_receipt_items_goods_receipts_goods_receipt_id");
    }

    public void Configure(EntityTypeBuilder<SupplierInvoice> builder)
    {
        builder.ToTable("supplier_invoices");
        ConfigureDocumentBase(builder);
        builder.Property(entity => entity.SupplierId).HasColumnName("supplier_id").IsRequired();
        builder.Property(entity => entity.PurchaseOrderId).HasColumnName("purchase_order_id");
        builder.Property(entity => entity.InvoiceNumber).HasColumnName("invoice_number").HasMaxLength(40).IsRequired();
        builder.Property(entity => entity.InvoiceDateUtc).HasColumnName("invoice_date_utc").IsRequired();
        builder.Property(entity => entity.Subtotal).HasColumnName("subtotal").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.TaxTotal).HasColumnName("tax_total").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.GrandTotal).HasColumnName("grand_total").HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.Status).HasColumnName("status").IsRequired();
        builder.Property(entity => entity.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.HasIndex(entity => new { entity.OrganisationId, entity.SupplierId, entity.InvoiceNumber }).IsUnique();
        RestrictSupplier(builder, "fk_supplier_invoices_suppliers_supplier_id");
        builder.HasOne<PurchaseOrder>().WithMany().HasForeignKey(entity => entity.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_supplier_invoices_purchase_orders_purchase_order_id");
    }

    private static void ConfigureDocumentBase<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.HasKey("Id");
        builder.Property<Guid>("Id").HasColumnName("id");
        builder.Property<Guid>("OrganisationId").HasColumnName("organisation_id").IsRequired();
        builder.Property<Guid>("BranchId").HasColumnName("branch_id").IsRequired();
        RestrictOrganisation(builder);
        builder.HasOne<Branch>().WithMany().HasForeignKey("BranchId")
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName($"fk_{builder.Metadata.GetTableName()}_branches_branch_id");
    }

    private static void ConfigureLineBase<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.HasKey("Id");
        builder.Property<Guid>("Id").HasColumnName("id");
        builder.Property<Guid>("OrganisationId").HasColumnName("organisation_id").IsRequired();
        builder.Property<Guid>("ProductId").HasColumnName("product_id").IsRequired();
        builder.Property<string>("Description").HasColumnName("description").HasMaxLength(160).IsRequired();
        builder.Property<decimal>("Quantity").HasColumnName("quantity").HasPrecision(18, 3).IsRequired();
        RestrictOrganisation(builder);
        builder.HasOne<Product>().WithMany().HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName($"fk_{builder.Metadata.GetTableName()}_products_product_id");
    }

    private static void RestrictOrganisation<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.HasOne<Organisation>().WithMany().HasForeignKey("OrganisationId")
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName($"fk_{builder.Metadata.GetTableName()}_organisations_organisation_id");
    }

    private static void RestrictSupplier<TEntity>(EntityTypeBuilder<TEntity> builder, string constraintName)
        where TEntity : class
    {
        builder.HasOne<Supplier>().WithMany().HasForeignKey("SupplierId")
            .OnDelete(DeleteBehavior.Restrict).HasConstraintName(constraintName);
    }
}
