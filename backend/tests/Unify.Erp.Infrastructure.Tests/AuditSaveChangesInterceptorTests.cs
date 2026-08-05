using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Unify.Erp.Application.Common;
using Unify.Erp.Domain.Audit;
using Unify.Erp.Domain.Branches;
using Unify.Erp.Domain.Customers;
using Unify.Erp.Domain.Organisations;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Tests;

public sealed class AuditSaveChangesInterceptorTests
{
    [Fact]
    public async Task Save_changes_adds_audit_entry_for_created_tenant_entity()
    {
        var context = new TestExecutionContext(Guid.NewGuid(), "auditor@unify.local", "correlation-create");
        using var dbContext = CreateDbContext(context);
        var (organisationId, branchId) = await CreateOrganisationAndBranchAsync(dbContext);

        var customer = new Customer(
            Guid.NewGuid(),
            organisationId,
            branchId,
            "C-100",
            "North Retail",
            null,
            null,
            null,
            null,
            0);

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        var auditEntry = await dbContext.AuditEntries.SingleAsync(entry => entry.EntityName == nameof(Customer));

        Assert.Equal(organisationId, auditEntry.OrganisationId);
        Assert.Equal(context.UserId, auditEntry.UserId);
        Assert.Equal(context.UserEmail, auditEntry.UserEmail);
        Assert.Equal(context.CorrelationId, auditEntry.CorrelationId);
        Assert.Equal(customer.Id.ToString(), auditEntry.EntityId);
        Assert.Equal(AuditOperation.Created, auditEntry.Operation);
        Assert.Null(auditEntry.OldValues);
        Assert.Contains(nameof(Customer.DisplayName), ReadChangedProperties(auditEntry));
    }

    [Fact]
    public async Task Save_changes_adds_audit_entry_for_updated_entity_with_old_and_new_values()
    {
        using var dbContext = CreateDbContext(new TestExecutionContext(null, null, null));
        var (organisationId, branchId) = await CreateOrganisationAndBranchAsync(dbContext);
        var customer = new Customer(
            Guid.NewGuid(),
            organisationId,
            branchId,
            "C-200",
            "South Retail",
            null,
            null,
            null,
            null,
            0);

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        customer.PutOnHold();
        await dbContext.SaveChangesAsync();

        var auditEntry = await dbContext.AuditEntries
            .Where(entry => entry.EntityName == nameof(Customer) && entry.Operation == AuditOperation.Updated)
            .SingleAsync();

        Assert.Contains(nameof(Customer.Status), ReadChangedProperties(auditEntry));
        Assert.Contains("Active", auditEntry.OldValues);
        Assert.Contains("OnHold", auditEntry.NewValues);
    }

    [Fact]
    public async Task Save_changes_adds_audit_entry_for_deleted_entity()
    {
        using var dbContext = CreateDbContext(new TestExecutionContext(null, null, "correlation-delete"));
        var organisation = new Organisation(Guid.NewGuid(), "Delete Test Org", "Delete Test Org", "USD", "UTC");
        dbContext.Organisations.Add(organisation);
        await dbContext.SaveChangesAsync();

        dbContext.Organisations.Remove(organisation);
        await dbContext.SaveChangesAsync();

        var auditEntry = await dbContext.AuditEntries
            .Where(entry => entry.EntityName == nameof(Organisation) && entry.Operation == AuditOperation.Deleted)
            .SingleAsync();

        Assert.Null(auditEntry.OrganisationId);
        Assert.Equal("correlation-delete", auditEntry.CorrelationId);
        Assert.Contains(nameof(Organisation.DisplayName), ReadChangedProperties(auditEntry));
        Assert.Null(auditEntry.NewValues);
    }

    private static ApplicationDbContext CreateDbContext(IExecutionContext executionContext)
    {
        var interceptor = new AuditSaveChangesInterceptor(executionContext);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<(Guid OrganisationId, Guid BranchId)> CreateOrganisationAndBranchAsync(ApplicationDbContext dbContext)
    {
        var organisation = new Organisation(Guid.NewGuid(), "Audit Test Org", "Audit Test Org", "USD", "UTC");
        var branch = new Branch(Guid.NewGuid(), organisation.Id, "MAIN", "Main", "UTC");

        dbContext.Organisations.Add(organisation);
        dbContext.Branches.Add(branch);
        await dbContext.SaveChangesAsync();

        return (organisation.Id, branch.Id);
    }

    private static string[] ReadChangedProperties(AuditEntry auditEntry)
    {
        return JsonSerializer.Deserialize<string[]>(auditEntry.ChangedProperties ?? "[]") ?? [];
    }

    private sealed record TestExecutionContext(Guid? UserId, string? UserEmail, string? CorrelationId) : IExecutionContext;
}
