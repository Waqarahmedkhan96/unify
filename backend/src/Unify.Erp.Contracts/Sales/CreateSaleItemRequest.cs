namespace Unify.Erp.Contracts.Sales;

public sealed record CreateSaleItemRequest(
    Guid ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal TaxAmount);
