using Unify.Erp.Domain.Purchasing;

namespace Unify.Erp.Domain.Tests;

public sealed class PurchasingTests
{
    [Fact]
    public void Purchase_order_normalizes_order_number()
    {
        var order = new PurchaseOrder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            " po-001 ",
            DateTimeOffset.UtcNow,
            100,
            5,
            105);

        Assert.Equal("PO-001", order.OrderNumber);
        Assert.Equal(PurchaseOrderStatus.Open, order.Status);
    }

    [Fact]
    public void Goods_receipt_normalizes_receipt_number()
    {
        var receipt = new GoodsReceipt(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            " grn-001 ",
            DateTimeOffset.UtcNow);

        Assert.Equal("GRN-001", receipt.ReceiptNumber);
    }
}
