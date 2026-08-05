using Unify.Erp.Domain.Sales;

namespace Unify.Erp.Domain.Tests;

public sealed class SalesTests
{
    [Fact]
    public void Sale_normalizes_invoice_number()
    {
        var sale = new Sale(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            " inv-001 ",
            DateTimeOffset.UtcNow,
            100,
            0,
            0,
            100);

        Assert.Equal("INV-001", sale.InvoiceNumber);
        Assert.Equal(SaleStatus.Posted, sale.Status);
    }

    [Fact]
    public void Sale_item_calculates_from_positive_quantity()
    {
        var item = new SaleItem(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "LPG Cylinder",
            2,
            100,
            10,
            5,
            195);

        Assert.Equal(195, item.LineTotal);
    }
}
