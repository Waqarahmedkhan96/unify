using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Purchasing;

public sealed class GoodsReceiptItem : TenantEntity
{
    public GoodsReceiptItem(Guid id, Guid organisationId, Guid goodsReceiptId, Guid productId, string description, decimal quantity)
        : base(id, organisationId)
    {
        GoodsReceiptId = Guard.RequiredId(goodsReceiptId, nameof(goodsReceiptId));
        ProductId = Guard.RequiredId(productId, nameof(productId));
        Description = Guard.RequiredText(description, nameof(description), 160);
        Quantity = Guard.PositiveQuantity(quantity, nameof(quantity));
    }

    public Guid GoodsReceiptId { get; }
    public Guid ProductId { get; }
    public string Description { get; }
    public decimal Quantity { get; }
}
