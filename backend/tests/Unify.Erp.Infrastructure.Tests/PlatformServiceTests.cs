using Microsoft.EntityFrameworkCore;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Platform;
using Unify.Erp.Infrastructure.Persistence;
using Unify.Erp.Infrastructure.Platform;

namespace Unify.Erp.Infrastructure.Tests;

public sealed class PlatformServiceTests
{
    [Fact]
    public async Task Creates_branch_under_existing_organisation()
    {
        using var dbContext = CreateDbContext();
        var organisationService = new OrganisationService(dbContext);
        var branchService = new BranchService(dbContext);
        var organisation = await organisationService.CreateAsync(
            new CreateOrganisationRequest("Royal LPG Private Limited", "Royal LPG", "PKR", "Asia/Karachi"),
            CancellationToken.None);

        var branch = await branchService.CreateAsync(
            new CreateBranchRequest(organisation.Id, "main", "Main Branch", "Asia/Karachi"),
            CancellationToken.None);

        Assert.Equal("MAIN", branch.Code);
        Assert.Equal(organisation.Id, branch.OrganisationId);
    }

    [Fact]
    public async Task Rejects_warehouse_for_branch_from_another_organisation()
    {
        using var dbContext = CreateDbContext();
        var organisationService = new OrganisationService(dbContext);
        var branchService = new BranchService(dbContext);
        var warehouseService = new WarehouseService(dbContext);
        var firstOrganisation = await organisationService.CreateAsync(
            new CreateOrganisationRequest("First Org", "First", "PKR", "Asia/Karachi"),
            CancellationToken.None);
        var secondOrganisation = await organisationService.CreateAsync(
            new CreateOrganisationRequest("Second Org", "Second", "PKR", "Asia/Karachi"),
            CancellationToken.None);
        var branch = await branchService.CreateAsync(
            new CreateBranchRequest(firstOrganisation.Id, "main", "Main Branch", "Asia/Karachi"),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            warehouseService.CreateAsync(
                new CreateWarehouseRequest(secondOrganisation.Id, branch.Id, "central", "Central Warehouse"),
                CancellationToken.None));

        Assert.Equal("Branch does not exist for the organisation.", exception.Message);
    }

    [Fact]
    public async Task Lists_organisations_with_paging_metadata()
    {
        using var dbContext = CreateDbContext();
        var organisationService = new OrganisationService(dbContext);

        await organisationService.CreateAsync(
            new CreateOrganisationRequest("Bravo Limited", "Bravo", "PKR", "Asia/Karachi"),
            CancellationToken.None);
        await organisationService.CreateAsync(
            new CreateOrganisationRequest("Alpha Limited", "Alpha", "PKR", "Asia/Karachi"),
            CancellationToken.None);

        var response = await organisationService.ListAsync(new PagedRequest(1, 1), CancellationToken.None);

        Assert.Equal(2, response.TotalCount);
        Assert.Equal(2, response.TotalPages);
        Assert.Equal("Alpha", response.Items.Single().DisplayName);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
