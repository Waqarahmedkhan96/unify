namespace Unify.Erp.Contracts.Purchasing;

public sealed record CreateGoodsReceiptRequest(
    Guid OrganisationId,
    Guid BranchId,
    Guid WarehouseId,
    Guid SupplierId,
    Guid? PurchaseOrderId,
    string ReceiptNumber,
    DateTimeOffset ReceiptDateUtc,
    IReadOnlyCollection<GoodsReceiptLineRequest> Items);
