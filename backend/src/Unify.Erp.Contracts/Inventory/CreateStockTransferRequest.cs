namespace Unify.Erp.Contracts.Inventory;

public sealed record CreateStockTransferRequest(
    Guid OrganisationId,
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    Guid ProductId,
    decimal Quantity,
    string? Notes);
