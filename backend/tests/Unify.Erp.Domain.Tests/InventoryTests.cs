using Unify.Erp.Domain.Inventory;

namespace Unify.Erp.Domain.Tests;

public sealed class InventoryTests
{
    [Fact]
    public void Stock_movement_exposes_signed_quantity()
    {
        var movement = new StockMovement(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            StockMovementType.AdjustmentOut,
            5,
            "Test",
            null,
            null,
            DateTimeOffset.UtcNow);

        Assert.Equal(-5, movement.SignedQuantity);
    }

    [Fact]
    public void Stock_balance_rejects_negative_quantity()
    {
        var balance = new StockBalance(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var exception = Assert.Throws<InvalidOperationException>(() => balance.Apply(-1));

        Assert.Equal("Stock balance cannot go negative.", exception.Message);
    }
}
