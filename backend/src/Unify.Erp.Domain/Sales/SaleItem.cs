using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Sales;

public sealed class SaleItem : TenantEntity
{
    public SaleItem(
        Guid id,
        Guid organisationId,
        Guid saleId,
        Guid productId,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal discountAmount,
        decimal taxAmount,
        decimal lineTotal)
        : base(id, organisationId)
    {
        SaleId = Guard.RequiredId(saleId, nameof(saleId));
        ProductId = Guard.RequiredId(productId, nameof(productId));
        Description = Guard.RequiredText(description, nameof(description), 160);
        Quantity = Guard.PositiveQuantity(quantity, nameof(quantity));
        UnitPrice = Guard.NonNegativeMoney(unitPrice, nameof(unitPrice));
        DiscountAmount = Guard.NonNegativeMoney(discountAmount, nameof(discountAmount));
        TaxAmount = Guard.NonNegativeMoney(taxAmount, nameof(taxAmount));
        LineTotal = Guard.NonNegativeMoney(lineTotal, nameof(lineTotal));
    }

    public Guid SaleId { get; }

    public Guid ProductId { get; }

    public string Description { get; }

    public decimal Quantity { get; }

    public decimal UnitPrice { get; }

    public decimal DiscountAmount { get; }

    public decimal TaxAmount { get; }

    public decimal LineTotal { get; }
}
