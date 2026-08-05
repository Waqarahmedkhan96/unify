using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Unify.Erp.Contracts.Customers;
using Unify.Erp.Contracts.Inventory;
using Unify.Erp.Contracts.Platform;
using Unify.Erp.Contracts.Products;
using Unify.Erp.Contracts.Sales;
using Unify.Erp.Infrastructure.Customers;
using Unify.Erp.Infrastructure.Inventory;
using Unify.Erp.Infrastructure.Persistence;
using Unify.Erp.Infrastructure.Platform;
using Unify.Erp.Infrastructure.Products;
using Unify.Erp.Infrastructure.Sales;

namespace Unify.Erp.Infrastructure.Tests;

public sealed class SalesServiceTests
{
    [Fact]
    public async Task Creates_sale_and_deducts_stock()
    {
        using var dbContext = CreateDbContext();
        var fixture = await CreateSalesFixtureAsync(dbContext);
        var inventoryService = new InventoryService(dbContext);
        var salesService = new SalesService(dbContext);
        await inventoryService.AdjustAsync(
            new CreateStockAdjustmentRequest(
                fixture.OrganisationId,
                fixture.WarehouseId,
                fixture.ProductId,
                "AdjustmentIn",
                10,
                null),
            CancellationToken.None);

        var sale = await salesService.CreateAsync(
            new CreateSaleRequest(
                fixture.OrganisationId,
                fixture.BranchId,
                fixture.WarehouseId,
                fixture.CustomerId,
                "inv-001",
                DateTimeOffset.UtcNow,
                [
                    new CreateSaleItemRequest(fixture.ProductId, "LPG Cylinder", 2, 100, 10, 5)
                ]),
            CancellationToken.None);
        var balances = await inventoryService.ListBalancesAsync(fixture.OrganisationId, fixture.WarehouseId, CancellationToken.None);

        Assert.Equal("INV-001", sale.InvoiceNumber);
        Assert.Equal(200, sale.Subtotal);
        Assert.Equal(195, sale.GrandTotal);
        Assert.Equal(8, balances.Single().QuantityOnHand);
    }

    [Fact]
    public async Task Rejects_sale_when_stock_would_go_negative()
    {
        using var dbContext = CreateDbContext();
        var fixture = await CreateSalesFixtureAsync(dbContext);
        var salesService = new SalesService(dbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            salesService.CreateAsync(
                new CreateSaleRequest(
                    fixture.OrganisationId,
                    fixture.BranchId,
                    fixture.WarehouseId,
                    fixture.CustomerId,
                    "inv-001",
                    DateTimeOffset.UtcNow,
                    [
                        new CreateSaleItemRequest(fixture.ProductId, "LPG Cylinder", 2, 100, 0, 0)
                    ]),
                CancellationToken.None));

        Assert.Equal("Stock balance cannot go negative.", exception.Message);
    }

    [Fact]
    public async Task Rejects_duplicate_invoice_number()
    {
        using var dbContext = CreateDbContext();
        var fixture = await CreateSalesFixtureAsync(dbContext);
        var inventoryService = new InventoryService(dbContext);
        var salesService = new SalesService(dbContext);
        await inventoryService.AdjustAsync(
            new CreateStockAdjustmentRequest(fixture.OrganisationId, fixture.WarehouseId, fixture.ProductId, "AdjustmentIn", 10, null),
            CancellationToken.None);
        var request = new CreateSaleRequest(
            fixture.OrganisationId,
            fixture.BranchId,
            fixture.WarehouseId,
            fixture.CustomerId,
            "inv-001",
            DateTimeOffset.UtcNow,
            [new CreateSaleItemRequest(fixture.ProductId, "LPG Cylinder", 1, 100, 0, 0)]);

        await salesService.CreateAsync(request, CancellationToken.None);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            salesService.CreateAsync(request, CancellationToken.None));

        Assert.Equal("Invoice number already exists for the organisation.", exception.Message);
    }

    private static async Task<SalesFixture> CreateSalesFixtureAsync(ApplicationDbContext dbContext)
    {
        var organisationService = new OrganisationService(dbContext);
        var branchService = new BranchService(dbContext);
        var warehouseService = new WarehouseService(dbContext);
        var productService = new ProductCatalogService(dbContext);
        var customerService = new CustomerService(dbContext);
        var organisation = await organisationService.CreateAsync(
            new CreateOrganisationRequest("Royal LPG Private Limited", "Royal LPG", "PKR", "Asia/Karachi"),
            CancellationToken.None);
        var branch = await branchService.CreateAsync(
            new CreateBranchRequest(organisation.Id, "main", "Main Branch", "Asia/Karachi"),
            CancellationToken.None);
        var warehouse = await warehouseService.CreateAsync(
            new CreateWarehouseRequest(organisation.Id, branch.Id, "main", "Main Warehouse"),
            CancellationToken.None);
        var unit = await productService.CreateUnitOfMeasureAsync(
            new CreateUnitOfMeasureRequest(organisation.Id, "kg", "Kilogram", 3),
            CancellationToken.None);
        var product = await productService.CreateProductAsync(
            new CreateProductRequest(organisation.Id, unit.Id, null, "lpg-001", "LPG Cylinder", null, 100, 120, true),
            CancellationToken.None);
        var customer = await customerService.CreateAsync(
            new CreateCustomerRequest(organisation.Id, branch.Id, "cust-001", "Walk-in Customer", null, null, null, null, 0),
            CancellationToken.None);

        return new SalesFixture(organisation.Id, branch.Id, warehouse.Id, customer.Id, product.Id);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed record SalesFixture(
        Guid OrganisationId,
        Guid BranchId,
        Guid WarehouseId,
        Guid CustomerId,
        Guid ProductId);
}
