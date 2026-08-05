using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Inventory;

public sealed class StockMovement : TenantEntity
{
    public StockMovement(
        Guid id,
        Guid organisationId,
        Guid branchId,
        Guid warehouseId,
        Guid productId,
        StockMovementType movementType,
        decimal quantity,
        string referenceType,
        Guid? referenceId,
        string? notes,
        DateTimeOffset occurredAtUtc)
        : base(id, organisationId)
    {
        BranchId = Guard.RequiredId(branchId, nameof(branchId));
        WarehouseId = Guard.RequiredId(warehouseId, nameof(warehouseId));
        ProductId = Guard.RequiredId(productId, nameof(productId));
        MovementType = movementType;
        Quantity = Guard.PositiveQuantity(quantity, nameof(quantity));
        ReferenceType = Guard.RequiredText(referenceType, nameof(referenceType), 60);
        ReferenceId = referenceId;
        Notes = Guard.OptionalText(notes, nameof(notes), 500);
        OccurredAtUtc = occurredAtUtc;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid BranchId { get; }

    public Guid WarehouseId { get; }

    public Guid ProductId { get; }

    public StockMovementType MovementType { get; }

    public decimal Quantity { get; }

    public string ReferenceType { get; }

    public Guid? ReferenceId { get; }

    public string? Notes { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public DateTimeOffset CreatedAtUtc { get; }

    public decimal SignedQuantity => MovementType is StockMovementType.AdjustmentOut
        or StockMovementType.TransferOut
        or StockMovementType.SaleIssue
        or StockMovementType.ReturnOut
            ? -Quantity
            : Quantity;
}
