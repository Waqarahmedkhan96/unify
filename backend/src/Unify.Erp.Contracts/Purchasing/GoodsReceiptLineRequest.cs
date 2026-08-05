namespace Unify.Erp.Contracts.Purchasing;

public sealed record GoodsReceiptLineRequest(Guid ProductId, string Description, decimal Quantity);
