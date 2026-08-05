using Unify.Erp.Domain.Users;

namespace Unify.Erp.Domain.Tests;

public sealed class UserModelTests
{
    [Fact]
    public void Platform_user_normalizes_email()
    {
        var user = new PlatformUser(Guid.NewGuid(), " Owner@Royal.test ", "Owner");

        Assert.Equal("owner@royal.test", user.Email);
        Assert.True(user.CanAuthenticate);
    }

    [Fact]
    public void Suspended_membership_is_not_active()
    {
        var membership = new UserOrganisationMembership(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        membership.Suspend();

        Assert.False(membership.IsActive);
        Assert.Equal(MembershipStatus.Suspended, membership.Status);
    }
}
