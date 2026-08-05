using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Purchasing;

public sealed class PurchaseOrderItem : TenantEntity
{
    public PurchaseOrderItem(
        Guid id,
        Guid organisationId,
        Guid purchaseOrderId,
        Guid productId,
        string description,
        decimal quantity,
        decimal unitCost,
        decimal taxAmount,
        decimal lineTotal)
        : base(id, organisationId)
    {
        PurchaseOrderId = Guard.RequiredId(purchaseOrderId, nameof(purchaseOrderId));
        ProductId = Guard.RequiredId(productId, nameof(productId));
        Description = Guard.RequiredText(description, nameof(description), 160);
        Quantity = Guard.PositiveQuantity(quantity, nameof(quantity));
        UnitCost = Guard.NonNegativeMoney(unitCost, nameof(unitCost));
        TaxAmount = Guard.NonNegativeMoney(taxAmount, nameof(taxAmount));
        LineTotal = Guard.NonNegativeMoney(lineTotal, nameof(lineTotal));
    }

    public Guid PurchaseOrderId { get; }
    public Guid ProductId { get; }
    public string Description { get; }
    public decimal Quantity { get; }
    public decimal UnitCost { get; }
    public decimal TaxAmount { get; }
    public decimal LineTotal { get; }
}
