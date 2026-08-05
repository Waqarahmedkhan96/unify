using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Purchasing;

public sealed class PurchaseOrder : TenantEntity
{
    public PurchaseOrder(
        Guid id,
        Guid organisationId,
        Guid branchId,
        Guid supplierId,
        string orderNumber,
        DateTimeOffset orderDateUtc,
        decimal subtotal,
        decimal taxTotal,
        decimal grandTotal,
        PurchaseOrderStatus status = PurchaseOrderStatus.Open)
        : base(id, organisationId)
    {
        BranchId = Guard.RequiredId(branchId, nameof(branchId));
        SupplierId = Guard.RequiredId(supplierId, nameof(supplierId));
        OrderNumber = Guard.RequiredText(orderNumber, nameof(orderNumber), 40).ToUpperInvariant();
        OrderDateUtc = orderDateUtc;
        Subtotal = Guard.NonNegativeMoney(subtotal, nameof(subtotal));
        TaxTotal = Guard.NonNegativeMoney(taxTotal, nameof(taxTotal));
        GrandTotal = Guard.NonNegativeMoney(grandTotal, nameof(grandTotal));
        Status = status;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid BranchId { get; }
    public Guid SupplierId { get; }
    public string OrderNumber { get; }
    public DateTimeOffset OrderDateUtc { get; }
    public decimal Subtotal { get; }
    public decimal TaxTotal { get; }
    public decimal GrandTotal { get; }
    public PurchaseOrderStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }

    public void MarkReceived() => Status = PurchaseOrderStatus.Received;
}
