using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Purchasing;

public sealed class GoodsReceipt : TenantEntity
{
    public GoodsReceipt(
        Guid id,
        Guid organisationId,
        Guid branchId,
        Guid warehouseId,
        Guid supplierId,
        Guid? purchaseOrderId,
        string receiptNumber,
        DateTimeOffset receiptDateUtc)
        : base(id, organisationId)
    {
        BranchId = Guard.RequiredId(branchId, nameof(branchId));
        WarehouseId = Guard.RequiredId(warehouseId, nameof(warehouseId));
        SupplierId = Guard.RequiredId(supplierId, nameof(supplierId));
        PurchaseOrderId = purchaseOrderId;
        ReceiptNumber = Guard.RequiredText(receiptNumber, nameof(receiptNumber), 40).ToUpperInvariant();
        ReceiptDateUtc = receiptDateUtc;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid BranchId { get; }
    public Guid WarehouseId { get; }
    public Guid SupplierId { get; }
    public Guid? PurchaseOrderId { get; }
    public string ReceiptNumber { get; }
    public DateTimeOffset ReceiptDateUtc { get; }
    public DateTimeOffset CreatedAtUtc { get; }
}
