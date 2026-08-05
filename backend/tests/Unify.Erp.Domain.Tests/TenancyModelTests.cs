using Unify.Erp.Domain.Branches;
using Unify.Erp.Domain.Warehouses;

namespace Unify.Erp.Domain.Tests;

public sealed class TenancyModelTests
{
    [Fact]
    public void Branch_requires_organisation_id()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Branch(Guid.NewGuid(), Guid.Empty, "main", "Main Branch", "Asia/Karachi"));

        Assert.Equal("organisationId", exception.ParamName);
    }

    [Fact]
    public void Branch_normalizes_code()
    {
        var branch = new Branch(Guid.NewGuid(), Guid.NewGuid(), " main ", "Main Branch", "Asia/Karachi");

        Assert.Equal("MAIN", branch.Code);
        Assert.True(branch.IsActive);
    }

    [Fact]
    public void Warehouse_requires_branch_id()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Warehouse(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, "central", "Central Warehouse"));

        Assert.Equal("branchId", exception.ParamName);
    }
}
