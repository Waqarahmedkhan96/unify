namespace Unify.Erp.Contracts.Purchasing;

public sealed record PurchaseOrderItemResponse(
    Guid Id,
    Guid ProductId,
    string Description,
    decimal Quantity,
    decimal UnitCost,
    decimal TaxAmount,
    decimal LineTotal);
