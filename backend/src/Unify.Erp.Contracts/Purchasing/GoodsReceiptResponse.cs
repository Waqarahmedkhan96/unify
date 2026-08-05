namespace Unify.Erp.Contracts.Purchasing;

public sealed record GoodsReceiptResponse(
    Guid Id,
    Guid OrganisationId,
    Guid BranchId,
    Guid WarehouseId,
    Guid SupplierId,
    Guid? PurchaseOrderId,
    string ReceiptNumber,
    DateTimeOffset ReceiptDateUtc,
    IReadOnlyCollection<GoodsReceiptItemResponse> Items);
