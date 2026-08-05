using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Unify.Erp.Contracts.Customers;
using Unify.Erp.Contracts.Inventory;
using Unify.Erp.Contracts.Payments;
using Unify.Erp.Contracts.Platform;
using Unify.Erp.Contracts.Products;
using Unify.Erp.Contracts.Sales;
using Unify.Erp.Infrastructure.Customers;
using Unify.Erp.Infrastructure.Inventory;
using Unify.Erp.Infrastructure.Payments;
using Unify.Erp.Infrastructure.Persistence;
using Unify.Erp.Infrastructure.Platform;
using Unify.Erp.Infrastructure.Products;
using Unify.Erp.Infrastructure.Sales;

namespace Unify.Erp.Infrastructure.Tests;

public sealed class CustomerPaymentServiceTests
{
    [Fact]
    public async Task Sale_and_payment_update_customer_balance()
    {
        using var dbContext = CreateDbContext();
        var fixture = await CreatePaidSaleFixtureAsync(dbContext);
        var paymentService = new CustomerPaymentService(dbContext);

        var beforePayment = await paymentService.GetBalanceAsync(fixture.OrganisationId, fixture.CustomerId, CancellationToken.None);
        var payment = await paymentService.CreateAsync(
            new CreateCustomerPaymentRequest(
                fixture.OrganisationId,
                fixture.BranchId,
                fixture.CustomerId,
                "rec-001",
                60,
                "Cash",
                DateTimeOffset.UtcNow,
                null,
                [new PaymentAllocationRequest(fixture.SaleId, 60)]),
            CancellationToken.None);
        var afterPayment = await paymentService.GetBalanceAsync(fixture.OrganisationId, fixture.CustomerId, CancellationToken.None);
        var ledger = await paymentService.ListLedgerAsync(fixture.OrganisationId, fixture.CustomerId, CancellationToken.None);

        Assert.Equal(120, beforePayment.Balance);
        Assert.Equal("REC-001", payment.ReceiptNumber);
        Assert.Equal(60, afterPayment.Balance);
        Assert.Equal(2, ledger.Count);
    }

    [Fact]
    public async Task Rejects_allocations_above_payment_amount()
    {
        using var dbContext = CreateDbContext();
        var fixture = await CreatePaidSaleFixtureAsync(dbContext);
        var paymentService = new CustomerPaymentService(dbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            paymentService.CreateAsync(
                new CreateCustomerPaymentRequest(
                    fixture.OrganisationId,
                    fixture.BranchId,
                    fixture.CustomerId,
                    "rec-001",
                    50,
                    "Cash",
                    DateTimeOffset.UtcNow,
                    null,
                    [new PaymentAllocationRequest(fixture.SaleId, 60)]),
                CancellationToken.None));

        Assert.Equal("Allocations cannot exceed payment amount.", exception.Message);
    }

    private static async Task<PaymentFixture> CreatePaidSaleFixtureAsync(ApplicationDbContext dbContext)
    {
        var organisationService = new OrganisationService(dbContext);
        var branchService = new BranchService(dbContext);
        var warehouseService = new WarehouseService(dbContext);
        var productService = new ProductCatalogService(dbContext);
        var customerService = new CustomerService(dbContext);
        var inventoryService = new InventoryService(dbContext);
        var salesService = new SalesService(dbContext);
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
        await inventoryService.AdjustAsync(
            new CreateStockAdjustmentRequest(organisation.Id, warehouse.Id, product.Id, "AdjustmentIn", 10, null),
            CancellationToken.None);
        var sale = await salesService.CreateAsync(
            new CreateSaleRequest(
                organisation.Id,
                branch.Id,
                warehouse.Id,
                customer.Id,
                "inv-001",
                DateTimeOffset.UtcNow,
                [new CreateSaleItemRequest(product.Id, "LPG Cylinder", 1, 120, 0, 0)]),
            CancellationToken.None);

        return new PaymentFixture(organisation.Id, branch.Id, customer.Id, sale.Id);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed record PaymentFixture(Guid OrganisationId, Guid BranchId, Guid CustomerId, Guid SaleId);
}
