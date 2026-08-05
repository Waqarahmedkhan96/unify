namespace Unify.Erp.Contracts.Inventory;

public sealed record CreateStockAdjustmentRequest(
    Guid OrganisationId,
    Guid WarehouseId,
    Guid ProductId,
    string MovementType,
    decimal Quantity,
    string? Notes);
