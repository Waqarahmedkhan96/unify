using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Inventory;

public sealed class StockBalance : TenantEntity
{
    public StockBalance(Guid id, Guid organisationId, Guid warehouseId, Guid productId)
        : base(id, organisationId)
    {
        WarehouseId = Guard.RequiredId(warehouseId, nameof(warehouseId));
        ProductId = Guard.RequiredId(productId, nameof(productId));
        QuantityOnHand = 0;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid WarehouseId { get; }

    public Guid ProductId { get; }

    public decimal QuantityOnHand { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void Apply(decimal signedQuantity)
    {
        var nextQuantity = QuantityOnHand + signedQuantity;
        if (nextQuantity < 0)
        {
            throw new InvalidOperationException("Stock balance cannot go negative.");
        }

        QuantityOnHand = nextQuantity;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }
}
