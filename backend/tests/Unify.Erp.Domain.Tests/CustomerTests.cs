using Unify.Erp.Domain.Customers;

namespace Unify.Erp.Domain.Tests;

public sealed class CustomerTests
{
    [Fact]
    public void Normalizes_customer_number()
    {
        var customer = new Customer(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            " cust-001 ",
            "Walk-in Customer",
            null,
            null,
            null,
            null,
            0);

        Assert.Equal("CUST-001", customer.CustomerNumber);
        Assert.True(customer.IsActive);
    }

    [Fact]
    public void Rejects_negative_credit_limit()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Customer(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "CUST-001",
                "Walk-in Customer",
                null,
                null,
                null,
                null,
                -1));

        Assert.Equal("creditLimit", exception.ParamName);
    }

    [Fact]
    public void Deactivation_preserves_customer_record()
    {
        var customer = new Customer(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CUST-001",
            "Walk-in Customer",
            null,
            null,
            null,
            null,
            0);

        customer.Deactivate();

        Assert.Equal(CustomerStatus.Inactive, customer.Status);
        Assert.False(customer.IsActive);
    }
}
