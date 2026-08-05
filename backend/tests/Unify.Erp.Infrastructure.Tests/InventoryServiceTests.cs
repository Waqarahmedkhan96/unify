using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Unify.Erp.Contracts.Inventory;
using Unify.Erp.Contracts.Platform;
using Unify.Erp.Contracts.Products;
using Unify.Erp.Domain.Inventory;
using Unify.Erp.Infrastructure.Inventory;
using Unify.Erp.Infrastructure.Persistence;
using Unify.Erp.Infrastructure.Platform;
using Unify.Erp.Infrastructure.Products;

namespace Unify.Erp.Infrastructure.Tests;

public sealed class InventoryServiceTests
{
    [Fact]
    public async Task Adjustment_in_creates_balance_and_movement()
    {
        using var dbContext = CreateDbContext();
        var fixture = await CreateInventoryFixtureAsync(dbContext);
        var inventoryService = new InventoryService(dbContext);

        var movement = await inventoryService.AdjustAsync(
            new CreateStockAdjustmentRequest(
                fixture.OrganisationId,
                fixture.SourceWarehouseId,
                fixture.ProductId,
                "AdjustmentIn",
                10,
                "Opening stock"),
            CancellationToken.None);
        var balances = await inventoryService.ListBalancesAsync(fixture.OrganisationId, fixture.SourceWarehouseId, CancellationToken.None);

        Assert.Equal(10, movement.SignedQuantity);
        Assert.Equal(10, balances.Single().QuantityOnHand);
    }

    [Fact]
    public async Task Adjustment_out_rejects_negative_balance()
    {
        using var dbContext = CreateDbContext();
        var fixture = await CreateInventoryFixtureAsync(dbContext);
        var inventoryService = new InventoryService(dbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            inventoryService.AdjustAsync(
                new CreateStockAdjustmentRequest(
                    fixture.OrganisationId,
                    fixture.SourceWarehouseId,
                    fixture.ProductId,
                    "AdjustmentOut",
                    1,
                    null),
                CancellationToken.None));

        Assert.Equal("Stock balance cannot go negative.", exception.Message);
    }

    [Fact]
    public async Task Transfer_creates_outbound_and_inbound_movements()
    {
        using var dbContext = CreateDbContext();
        var fixture = await CreateInventoryFixtureAsync(dbContext);
        var inventoryService = new InventoryService(dbContext);
        await inventoryService.AdjustAsync(
            new CreateStockAdjustmentRequest(
                fixture.OrganisationId,
                fixture.SourceWarehouseId,
                fixture.ProductId,
                "AdjustmentIn",
                10,
                null),
            CancellationToken.None);

        var transfer = await inventoryService.TransferAsync(
            new CreateStockTransferRequest(
                fixture.OrganisationId,
                fixture.SourceWarehouseId,
                fixture.DestinationWarehouseId,
                fixture.ProductId,
                4,
                "Move to branch"),
            CancellationToken.None);
        var balances = await inventoryService.ListBalancesAsync(fixture.OrganisationId, null, CancellationToken.None);

        Assert.Equal(-4, transfer.SourceMovement.SignedQuantity);
        Assert.Equal(4, transfer.DestinationMovement.SignedQuantity);
        Assert.Equal(6, balances.Single(balance => balance.WarehouseId == fixture.SourceWarehouseId).QuantityOnHand);
        Assert.Equal(4, balances.Single(balance => balance.WarehouseId == fixture.DestinationWarehouseId).QuantityOnHand);
    }

    private static async Task<InventoryFixture> CreateInventoryFixtureAsync(ApplicationDbContext dbContext)
    {
        var organisationService = new OrganisationService(dbContext);
        var branchService = new BranchService(dbContext);
        var warehouseService = new WarehouseService(dbContext);
        var productService = new ProductCatalogService(dbContext);
        var organisation = await organisationService.CreateAsync(
            new CreateOrganisationRequest("Royal LPG Private Limited", "Royal LPG", "PKR", "Asia/Karachi"),
            CancellationToken.None);
        var branch = await branchService.CreateAsync(
            new CreateBranchRequest(organisation.Id, "main", "Main Branch", "Asia/Karachi"),
            CancellationToken.None);
        var sourceWarehouse = await warehouseService.CreateAsync(
            new CreateWarehouseRequest(organisation.Id, branch.Id, "src", "Source"),
            CancellationToken.None);
        var destinationWarehouse = await warehouseService.CreateAsync(
            new CreateWarehouseRequest(organisation.Id, branch.Id, "dst", "Destination"),
            CancellationToken.None);
        var unit = await productService.CreateUnitOfMeasureAsync(
            new CreateUnitOfMeasureRequest(organisation.Id, "kg", "Kilogram", 3),
            CancellationToken.None);
        var product = await productService.CreateProductAsync(
            new CreateProductRequest(organisation.Id, unit.Id, null, "lpg-001", "LPG Cylinder", null, 100, 120, true),
            CancellationToken.None);

        return new InventoryFixture(organisation.Id, sourceWarehouse.Id, destinationWarehouse.Id, product.Id);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed record InventoryFixture(
        Guid OrganisationId,
        Guid SourceWarehouseId,
        Guid DestinationWarehouseId,
        Guid ProductId);
}
