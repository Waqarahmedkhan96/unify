using Unify.Erp.Domain.Products;

namespace Unify.Erp.Domain.Tests;

public sealed class ProductCatalogTests
{
    [Fact]
    public void Unit_of_measure_normalizes_code()
    {
        var unit = new UnitOfMeasure(Guid.NewGuid(), Guid.NewGuid(), " kg ", "Kilogram", 3);

        Assert.Equal("KG", unit.Code);
        Assert.Equal(3, unit.DecimalPlaces);
    }

    [Fact]
    public void Product_rejects_negative_price()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Product(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                "LPG-001",
                "LPG Cylinder",
                null,
                -1,
                100,
                true));

        Assert.Equal("purchasePrice", exception.ParamName);
    }

    [Fact]
    public void Product_deactivation_preserves_record()
    {
        var product = new Product(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "LPG-001",
            "LPG Cylinder",
            null,
            50,
            100,
            true);

        product.Deactivate();

        Assert.Equal(ProductStatus.Inactive, product.Status);
        Assert.False(product.IsActive);
    }
}
