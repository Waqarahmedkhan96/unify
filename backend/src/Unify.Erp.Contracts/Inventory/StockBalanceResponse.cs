namespace Unify.Erp.Contracts.Inventory;

public sealed record StockBalanceResponse(
    Guid Id,
    Guid OrganisationId,
    Guid WarehouseId,
    Guid ProductId,
    decimal QuantityOnHand,
    DateTimeOffset UpdatedAtUtc);
