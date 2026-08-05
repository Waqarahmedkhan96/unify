using Unify.Erp.Domain.Organisations;

namespace Unify.Erp.Domain.Tests;

public sealed class OrganisationTests
{
    [Fact]
    public void Constructor_normalizes_currency()
    {
        var organisation = new Organisation(
            Guid.NewGuid(),
            "Royal LPG Private Limited",
            "Royal LPG",
            "pkr",
            "Asia/Karachi");

        Assert.Equal("PKR", organisation.BaseCurrency);
        Assert.True(organisation.IsActive);
    }

    [Fact]
    public void Constructor_rejects_empty_id()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Organisation(Guid.Empty, "Legal", "Display", "PKR", "Asia/Karachi"));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Suspend_marks_organisation_inactive()
    {
        var organisation = new Organisation(
            Guid.NewGuid(),
            "Legal",
            "Display",
            "PKR",
            "Asia/Karachi");

        organisation.Suspend();

        Assert.Equal(OrganisationStatus.Suspended, organisation.Status);
        Assert.False(organisation.IsActive);
    }
}
