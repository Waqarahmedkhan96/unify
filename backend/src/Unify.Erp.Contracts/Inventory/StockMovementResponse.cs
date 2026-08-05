namespace Unify.Erp.Contracts.Inventory;

public sealed record StockMovementResponse(
    Guid Id,
    Guid OrganisationId,
    Guid BranchId,
    Guid WarehouseId,
    Guid ProductId,
    string MovementType,
    decimal Quantity,
    decimal SignedQuantity,
    string ReferenceType,
    Guid? ReferenceId,
    string? Notes,
    DateTimeOffset OccurredAtUtc);
