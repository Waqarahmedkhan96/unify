namespace Unify.Erp.Contracts.Purchasing;

public sealed record GoodsReceiptItemResponse(Guid Id, Guid ProductId, string Description, decimal Quantity);
