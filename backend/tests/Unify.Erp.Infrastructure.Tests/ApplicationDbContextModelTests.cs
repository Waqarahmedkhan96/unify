using Microsoft.EntityFrameworkCore;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Tests;

public sealed class ApplicationDbContextModelTests
{
    [Fact]
    public void Model_contains_foundation_entities()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=unify_erp;Username=unify_app;Password=not_used")
            .Options;

        using var dbContext = new ApplicationDbContext(options);

        var entityTableNames = dbContext.Model
            .GetEntityTypes()
            .Select(entityType => entityType.GetTableName())
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("organisations", entityTableNames);
        Assert.Contains("branches", entityTableNames);
        Assert.Contains("warehouses", entityTableNames);
        Assert.Contains("devices", entityTableNames);
        Assert.Contains("device_sessions", entityTableNames);
        Assert.Contains("platform_users", entityTableNames);
    }
}
