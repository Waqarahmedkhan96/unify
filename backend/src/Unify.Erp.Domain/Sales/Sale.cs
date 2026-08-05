using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Sales;

public sealed class Sale : TenantEntity
{
    public Sale(
        Guid id,
        Guid organisationId,
        Guid branchId,
        Guid warehouseId,
        Guid customerId,
        string invoiceNumber,
        DateTimeOffset saleDateUtc,
        decimal subtotal,
        decimal discountTotal,
        decimal taxTotal,
        decimal grandTotal,
        SaleStatus status = SaleStatus.Posted)
        : base(id, organisationId)
    {
        BranchId = Guard.RequiredId(branchId, nameof(branchId));
        WarehouseId = Guard.RequiredId(warehouseId, nameof(warehouseId));
        CustomerId = Guard.RequiredId(customerId, nameof(customerId));
        InvoiceNumber = Guard.RequiredText(invoiceNumber, nameof(invoiceNumber), 40).ToUpperInvariant();
        SaleDateUtc = saleDateUtc;
        Subtotal = Guard.NonNegativeMoney(subtotal, nameof(subtotal));
        DiscountTotal = Guard.NonNegativeMoney(discountTotal, nameof(discountTotal));
        TaxTotal = Guard.NonNegativeMoney(taxTotal, nameof(taxTotal));
        GrandTotal = Guard.NonNegativeMoney(grandTotal, nameof(grandTotal));
        Status = status;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid BranchId { get; }

    public Guid WarehouseId { get; }

    public Guid CustomerId { get; }

    public string InvoiceNumber { get; }

    public DateTimeOffset SaleDateUtc { get; }

    public decimal Subtotal { get; }

    public decimal DiscountTotal { get; }

    public decimal TaxTotal { get; }

    public decimal GrandTotal { get; }

    public SaleStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public void Void()
    {
        Status = SaleStatus.Voided;
    }
}
