using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Unify.Erp.Contracts.Inventory;
using Unify.Erp.Contracts.Platform;
using Unify.Erp.Contracts.Products;
using Unify.Erp.Contracts.Purchasing;
using Unify.Erp.Contracts.Suppliers;
using Unify.Erp.Infrastructure.Inventory;
using Unify.Erp.Infrastructure.Persistence;
using Unify.Erp.Infrastructure.Platform;
using Unify.Erp.Infrastructure.Products;
using Unify.Erp.Infrastructure.Purchasing;
using Unify.Erp.Infrastructure.Suppliers;

namespace Unify.Erp.Infrastructure.Tests;

public sealed class PurchasingServiceTests
{
    [Fact]
    public async Task Creates_purchase_order_with_server_totals()
    {
        using var dbContext = CreateDbContext();
        var fixture = await CreateFixtureAsync(dbContext);
        var purchasingService = new PurchasingService(dbContext);

        var order = await purchasingService.CreatePurchaseOrderAsync(
            new CreatePurchaseOrderRequest(
                fixture.OrganisationId,
                fixture.BranchId,
                fixture.SupplierId,
                "po-001",
                DateTimeOffset.UtcNow,
                [new PurchaseLineRequest(fixture.ProductId, "LPG Cylinder", 2, 100, 10)]),
            CancellationToken.None);

        Assert.Equal("PO-001", order.OrderNumber);
        Assert.Equal(200, order.Subtotal);
        Assert.Equal(210, order.GrandTotal);
    }

    [Fact]
    public async Task Goods_receipt_increases_stock_and_marks_order_received()
    {
        using var dbContext = CreateDbContext();
        var fixture = await CreateFixtureAsync(dbContext);
        var purchasingService = new PurchasingService(dbContext);
        var inventoryService = new InventoryService(dbContext);
        var order = await purchasingService.CreatePurchaseOrderAsync(
            new CreatePurchaseOrderRequest(
                fixture.OrganisationId,
                fixture.BranchId,
                fixture.SupplierId,
                "po-001",
                DateTimeOffset.UtcNow,
                [new PurchaseLineRequest(fixture.ProductId, "LPG Cylinder", 5, 100, 0)]),
            CancellationToken.None);

        var receipt = await purchasingService.CreateGoodsReceiptAsync(
            new CreateGoodsReceiptRequest(
                fixture.OrganisationId,
                fixture.BranchId,
                fixture.WarehouseId,
                fixture.SupplierId,
                order.Id,
                "grn-001",
                DateTimeOffset.UtcNow,
                [new GoodsReceiptLineRequest(fixture.ProductId, "LPG Cylinder", 5)]),
            CancellationToken.None);
        var balances = await inventoryService.ListBalancesAsync(fixture.OrganisationId, fixture.WarehouseId, CancellationToken.None);
        var orders = await purchasingService.ListPurchaseOrdersAsync(
            new ListPurchasingDocumentsRequest(fixture.OrganisationId, fixture.SupplierId, new(1, 10)),
            CancellationToken.None);

        Assert.Equal("GRN-001", receipt.ReceiptNumber);
        Assert.Equal(5, balances.Single().QuantityOnHand);
        Assert.Equal("Received", orders.Items.Single().Status);
    }

    [Fact]
    public async Task Rejects_duplicate_supplier_invoice_number_per_supplier()
    {
        using var dbContext = CreateDbContext();
        var fixture = await CreateFixtureAsync(dbContext);
        var purchasingService = new PurchasingService(dbContext);
        var request = new CreateSupplierInvoiceRequest(
            fixture.OrganisationId,
            fixture.BranchId,
            fixture.SupplierId,
            null,
            "sinv-001",
            DateTimeOffset.UtcNow,
            [new PurchaseLineRequest(fixture.ProductId, "LPG Cylinder", 1, 100, 0)]);

        await purchasingService.CreateSupplierInvoiceAsync(request, CancellationToken.None);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            purchasingService.CreateSupplierInvoiceAsync(request, CancellationToken.None));

        Assert.Equal("Supplier invoice number already exists for the supplier.", exception.Message);
    }

    private static async Task<PurchasingFixture> CreateFixtureAsync(ApplicationDbContext dbContext)
    {
        var organisationService = new OrganisationService(dbContext);
        var branchService = new BranchService(dbContext);
        var warehouseService = new WarehouseService(dbContext);
        var productService = new ProductCatalogService(dbContext);
        var supplierService = new SupplierService(dbContext);
        var organisation = await organisationService.CreateAsync(
            new CreateOrganisationRequest("Royal LPG Private Limited", "Royal LPG", "PKR", "Asia/Karachi"),
            CancellationToken.None);
        var branch = await branchService.CreateAsync(
            new CreateBranchRequest(organisation.Id, "main", "Main Branch", "Asia/Karachi"),
            CancellationToken.None);
        var warehouse = await warehouseService.CreateAsync(
            new CreateWarehouseRequest(organisation.Id, branch.Id, "main", "Main Warehouse"),
            CancellationToken.None);
        var supplier = await supplierService.CreateAsync(
            new CreateSupplierRequest(organisation.Id, "sup-001", "Cylinder Supplier", null, null, null, null),
            CancellationToken.None);
        var unit = await productService.CreateUnitOfMeasureAsync(
            new CreateUnitOfMeasureRequest(organisation.Id, "kg", "Kilogram", 3),
            CancellationToken.None);
        var product = await productService.CreateProductAsync(
            new CreateProductRequest(organisation.Id, unit.Id, null, "lpg-001", "LPG Cylinder", null, 100, 120, true),
            CancellationToken.None);

        return new PurchasingFixture(organisation.Id, branch.Id, warehouse.Id, supplier.Id, product.Id);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed record PurchasingFixture(
        Guid OrganisationId,
        Guid BranchId,
        Guid WarehouseId,
        Guid SupplierId,
        Guid ProductId);
}
