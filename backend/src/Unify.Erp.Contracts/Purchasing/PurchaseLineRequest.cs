namespace Unify.Erp.Contracts.Purchasing;

public sealed record PurchaseLineRequest(
    Guid ProductId,
    string Description,
    decimal Quantity,
    decimal UnitCost,
    decimal TaxAmount);
