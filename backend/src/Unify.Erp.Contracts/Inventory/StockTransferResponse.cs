namespace Unify.Erp.Contracts.Inventory;

public sealed record StockTransferResponse(
    Guid TransferId,
    StockMovementResponse SourceMovement,
    StockMovementResponse DestinationMovement);
