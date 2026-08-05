using Unify.Erp.Domain.Suppliers;

namespace Unify.Erp.Domain.Tests;

public sealed class SupplierTests
{
    [Fact]
    public void Normalizes_supplier_number()
    {
        var supplier = new Supplier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            " sup-001 ",
            "Cylinder Supplier",
            null,
            null,
            null,
            null);

        Assert.Equal("SUP-001", supplier.SupplierNumber);
        Assert.True(supplier.IsActive);
    }

    [Fact]
    public void Deactivation_preserves_supplier_record()
    {
        var supplier = new Supplier(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SUP-001",
            "Cylinder Supplier",
            null,
            null,
            null,
            null);

        supplier.Deactivate();

        Assert.Equal(SupplierStatus.Inactive, supplier.Status);
        Assert.False(supplier.IsActive);
    }
}
