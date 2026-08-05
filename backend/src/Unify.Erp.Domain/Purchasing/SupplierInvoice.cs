using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Purchasing;

public sealed class SupplierInvoice : TenantEntity
{
    public SupplierInvoice(
        Guid id,
        Guid organisationId,
        Guid branchId,
        Guid supplierId,
        Guid? purchaseOrderId,
        string invoiceNumber,
        DateTimeOffset invoiceDateUtc,
        decimal subtotal,
        decimal taxTotal,
        decimal grandTotal,
        SupplierInvoiceStatus status = SupplierInvoiceStatus.Posted)
        : base(id, organisationId)
    {
        BranchId = Guard.RequiredId(branchId, nameof(branchId));
        SupplierId = Guard.RequiredId(supplierId, nameof(supplierId));
        PurchaseOrderId = purchaseOrderId;
        InvoiceNumber = Guard.RequiredText(invoiceNumber, nameof(invoiceNumber), 40).ToUpperInvariant();
        InvoiceDateUtc = invoiceDateUtc;
        Subtotal = Guard.NonNegativeMoney(subtotal, nameof(subtotal));
        TaxTotal = Guard.NonNegativeMoney(taxTotal, nameof(taxTotal));
        GrandTotal = Guard.NonNegativeMoney(grandTotal, nameof(grandTotal));
        Status = status;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid BranchId { get; }
    public Guid SupplierId { get; }
    public Guid? PurchaseOrderId { get; }
    public string InvoiceNumber { get; }
    public DateTimeOffset InvoiceDateUtc { get; }
    public decimal Subtotal { get; }
    public decimal TaxTotal { get; }
    public decimal GrandTotal { get; }
    public SupplierInvoiceStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
}
