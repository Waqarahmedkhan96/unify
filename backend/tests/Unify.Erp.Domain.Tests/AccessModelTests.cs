using Unify.Erp.Domain.Access;

namespace Unify.Erp.Domain.Tests;

public sealed class AccessModelTests
{
    [Fact]
    public void Role_grants_permissions_case_insensitively()
    {
        var role = new Role(Guid.NewGuid(), Guid.NewGuid(), "Cashier");

        role.Grant("Sales.Create");

        Assert.True(role.HasPermission("sales.create"));
    }

    [Fact]
    public void Permission_normalizes_key()
    {
        var permission = new Permission(Guid.NewGuid(), " Customers.View ", "View customers");

        Assert.Equal("customers.view", permission.Key);
    }
}
