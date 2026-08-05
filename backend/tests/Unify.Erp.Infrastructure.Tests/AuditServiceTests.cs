using Microsoft.EntityFrameworkCore;
using Unify.Erp.Contracts.Audit;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Domain.Audit;
using Unify.Erp.Infrastructure.Audit;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Tests;

public sealed class AuditServiceTests
{
    [Fact]
    public async Task List_async_filters_by_organisation_and_entity_name()
    {
        using var dbContext = CreateDbContext();
        var organisationId = Guid.NewGuid();

        dbContext.AuditEntries.AddRange(
            new AuditEntry(
                Guid.NewGuid(),
                organisationId,
                Guid.NewGuid(),
                "auditor@unify.local",
                "correlation-1",
                "Customer",
                Guid.NewGuid().ToString(),
                AuditOperation.Created,
                "[\"DisplayName\"]",
                null,
                "{\"displayName\":\"North Retail\"}",
                DateTimeOffset.UtcNow),
            new AuditEntry(
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                null,
                "correlation-2",
                "Supplier",
                Guid.NewGuid().ToString(),
                AuditOperation.Created,
                "[\"DisplayName\"]",
                null,
                "{\"displayName\":\"Supply Co\"}",
                DateTimeOffset.UtcNow));
        await dbContext.SaveChangesAsync();

        var auditService = new AuditService(dbContext);
        var response = await auditService.ListAsync(
            new ListAuditEntriesRequest(organisationId, "Customer", null, null, null, new PagedRequest(1, 10)),
            CancellationToken.None);

        var auditEntry = Assert.Single(response.Items);
        Assert.Equal("Customer", auditEntry.EntityName);
        Assert.Equal(organisationId, auditEntry.OrganisationId);
        Assert.Equal(1, response.TotalCount);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
