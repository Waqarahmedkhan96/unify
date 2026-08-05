using Microsoft.EntityFrameworkCore;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Platform;
using Unify.Erp.Contracts.Suppliers;
using Unify.Erp.Infrastructure.Persistence;
using Unify.Erp.Infrastructure.Platform;
using Unify.Erp.Infrastructure.Suppliers;

namespace Unify.Erp.Infrastructure.Tests;

public sealed class SupplierServiceTests
{
    [Fact]
    public async Task Creates_supplier_under_existing_organisation()
    {
        using var dbContext = CreateDbContext();
        var organisationService = new OrganisationService(dbContext);
        var supplierService = new SupplierService(dbContext);
        var organisation = await organisationService.CreateAsync(
            new CreateOrganisationRequest("Royal LPG Private Limited", "Royal LPG", "PKR", "Asia/Karachi"),
            CancellationToken.None);

        var supplier = await supplierService.CreateAsync(
            new CreateSupplierRequest(
                organisation.Id,
                "sup-001",
                "Cylinder Supplier",
                null,
                "03000000000",
                null,
                null),
            CancellationToken.None);

        Assert.Equal("SUP-001", supplier.SupplierNumber);
        Assert.Equal(organisation.Id, supplier.OrganisationId);
    }

    [Fact]
    public async Task Rejects_duplicate_supplier_number_per_organisation()
    {
        using var dbContext = CreateDbContext();
        var organisationService = new OrganisationService(dbContext);
        var supplierService = new SupplierService(dbContext);
        var organisation = await organisationService.CreateAsync(
            new CreateOrganisationRequest("Royal LPG Private Limited", "Royal LPG", "PKR", "Asia/Karachi"),
            CancellationToken.None);
        var request = new CreateSupplierRequest(
            organisation.Id,
            "sup-001",
            "Cylinder Supplier",
            null,
            null,
            null,
            null);

        await supplierService.CreateAsync(request, CancellationToken.None);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            supplierService.CreateAsync(request, CancellationToken.None));

        Assert.Equal("Supplier number already exists for the organisation.", exception.Message);
    }

    [Fact]
    public async Task Lists_suppliers_with_search_and_paging()
    {
        using var dbContext = CreateDbContext();
        var organisationService = new OrganisationService(dbContext);
        var supplierService = new SupplierService(dbContext);
        var organisation = await organisationService.CreateAsync(
            new CreateOrganisationRequest("Royal LPG Private Limited", "Royal LPG", "PKR", "Asia/Karachi"),
            CancellationToken.None);
        await supplierService.CreateAsync(
            new CreateSupplierRequest(organisation.Id, "sup-001", "Alpha Cylinders", null, null, null, null),
            CancellationToken.None);
        await supplierService.CreateAsync(
            new CreateSupplierRequest(organisation.Id, "sup-002", "Bravo Gas", null, null, null, null),
            CancellationToken.None);

        var response = await supplierService.ListAsync(
            new ListSuppliersRequest(organisation.Id, "Bravo", new PagedRequest(1, 10)),
            CancellationToken.None);

        Assert.Equal(1, response.TotalCount);
        Assert.Equal("Bravo Gas", response.Items.Single().DisplayName);
    }

    [Fact]
    public async Task Deactivates_supplier_without_deleting_record()
    {
        using var dbContext = CreateDbContext();
        var organisationService = new OrganisationService(dbContext);
        var supplierService = new SupplierService(dbContext);
        var organisation = await organisationService.CreateAsync(
            new CreateOrganisationRequest("Royal LPG Private Limited", "Royal LPG", "PKR", "Asia/Karachi"),
            CancellationToken.None);
        var supplier = await supplierService.CreateAsync(
            new CreateSupplierRequest(organisation.Id, "sup-001", "Alpha Cylinders", null, null, null, null),
            CancellationToken.None);

        var deactivated = await supplierService.DeactivateAsync(organisation.Id, supplier.Id, CancellationToken.None);
        var fetched = await supplierService.GetAsync(organisation.Id, supplier.Id, CancellationToken.None);

        Assert.True(deactivated);
        Assert.Equal("Inactive", fetched?.Status);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
