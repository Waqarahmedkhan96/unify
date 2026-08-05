using Microsoft.EntityFrameworkCore;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Platform;
using Unify.Erp.Contracts.Products;
using Unify.Erp.Infrastructure.Persistence;
using Unify.Erp.Infrastructure.Platform;
using Unify.Erp.Infrastructure.Products;

namespace Unify.Erp.Infrastructure.Tests;

public sealed class ProductCatalogServiceTests
{
    [Fact]
    public async Task Creates_product_with_unit_and_category()
    {
        using var dbContext = CreateDbContext();
        var organisationService = new OrganisationService(dbContext);
        var catalogService = new ProductCatalogService(dbContext);
        var organisation = await organisationService.CreateAsync(
            new CreateOrganisationRequest("Royal LPG Private Limited", "Royal LPG", "PKR", "Asia/Karachi"),
            CancellationToken.None);
        var unit = await catalogService.CreateUnitOfMeasureAsync(
            new CreateUnitOfMeasureRequest(organisation.Id, "kg", "Kilogram", 3),
            CancellationToken.None);
        var category = await catalogService.CreateCategoryAsync(
            new CreateProductCategoryRequest(organisation.Id, "gas", "Gas"),
            CancellationToken.None);

        var product = await catalogService.CreateProductAsync(
            new CreateProductRequest(
                organisation.Id,
                unit.Id,
                category.Id,
                "lpg-001",
                "LPG Cylinder",
                "123456",
                2500,
                3000,
                true),
            CancellationToken.None);

        Assert.Equal("LPG-001", product.ProductCode);
        Assert.Equal(unit.Id, product.UnitOfMeasureId);
        Assert.Equal(category.Id, product.CategoryId);
    }

    [Fact]
    public async Task Rejects_duplicate_product_code_per_organisation()
    {
        using var dbContext = CreateDbContext();
        var organisationService = new OrganisationService(dbContext);
        var catalogService = new ProductCatalogService(dbContext);
        var organisation = await organisationService.CreateAsync(
            new CreateOrganisationRequest("Royal LPG Private Limited", "Royal LPG", "PKR", "Asia/Karachi"),
            CancellationToken.None);
        var unit = await catalogService.CreateUnitOfMeasureAsync(
            new CreateUnitOfMeasureRequest(organisation.Id, "kg", "Kilogram", 3),
            CancellationToken.None);
        var request = new CreateProductRequest(
            organisation.Id,
            unit.Id,
            null,
            "lpg-001",
            "LPG Cylinder",
            null,
            2500,
            3000,
            true);

        await catalogService.CreateProductAsync(request, CancellationToken.None);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            catalogService.CreateProductAsync(request, CancellationToken.None));

        Assert.Equal("Product code already exists for the organisation.", exception.Message);
    }

    [Fact]
    public async Task Lists_products_with_search_and_paging()
    {
        using var dbContext = CreateDbContext();
        var organisationService = new OrganisationService(dbContext);
        var catalogService = new ProductCatalogService(dbContext);
        var organisation = await organisationService.CreateAsync(
            new CreateOrganisationRequest("Royal LPG Private Limited", "Royal LPG", "PKR", "Asia/Karachi"),
            CancellationToken.None);
        var unit = await catalogService.CreateUnitOfMeasureAsync(
            new CreateUnitOfMeasureRequest(organisation.Id, "kg", "Kilogram", 3),
            CancellationToken.None);
        await catalogService.CreateProductAsync(
            new CreateProductRequest(organisation.Id, unit.Id, null, "lpg-001", "LPG Cylinder", null, 2500, 3000, true),
            CancellationToken.None);

        var response = await catalogService.ListProductsAsync(
            new ListProductsRequest(organisation.Id, null, "Cylinder", new PagedRequest(1, 10)),
            CancellationToken.None);

        Assert.Equal(1, response.TotalCount);
        Assert.Equal("LPG Cylinder", response.Items.Single().Name);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
