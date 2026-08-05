using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Products;

public sealed class Product : TenantEntity
{
    public Product(
        Guid id,
        Guid organisationId,
        Guid unitOfMeasureId,
        Guid? categoryId,
        string productCode,
        string name,
        string? barcode,
        decimal purchasePrice,
        decimal salesPrice,
        bool isInventoryTracked,
        ProductStatus status = ProductStatus.Active)
        : base(id, organisationId)
    {
        UnitOfMeasureId = Guard.RequiredId(unitOfMeasureId, nameof(unitOfMeasureId));
        CategoryId = categoryId;
        ProductCode = Guard.RequiredText(productCode, nameof(productCode), 32).ToUpperInvariant();
        Name = Guard.RequiredText(name, nameof(name), 160);
        Barcode = Guard.OptionalText(barcode, nameof(barcode), 80);
        PurchasePrice = Guard.NonNegativeMoney(purchasePrice, nameof(purchasePrice));
        SalesPrice = Guard.NonNegativeMoney(salesPrice, nameof(salesPrice));
        IsInventoryTracked = isInventoryTracked;
        Status = status;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid UnitOfMeasureId { get; }

    public Guid? CategoryId { get; }

    public string ProductCode { get; }

    public string Name { get; }

    public string? Barcode { get; }

    public decimal PurchasePrice { get; }

    public decimal SalesPrice { get; }

    public bool IsInventoryTracked { get; }

    public ProductStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public bool IsActive => Status == ProductStatus.Active;

    public void Deactivate()
    {
        Status = ProductStatus.Inactive;
    }
}
