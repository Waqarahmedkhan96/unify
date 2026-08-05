namespace Unify.Erp.Contracts.Sales;

public sealed record SaleItemResponse(
    Guid Id,
    Guid ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal LineTotal);
