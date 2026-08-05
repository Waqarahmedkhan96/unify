using Unify.Erp.Domain.Devices;
using Unify.Erp.Domain.Sessions;

namespace Unify.Erp.Domain.Tests;

public sealed class DeviceAndSessionTests
{
    [Fact]
    public void Device_cannot_synchronize_until_approved()
    {
        var device = new Device(Guid.NewGuid(), Guid.NewGuid(), "Front counter", DeviceType.Windows);

        Assert.False(device.CanSynchronize);

        device.Approve();

        Assert.True(device.CanSynchronize);
    }

    [Fact]
    public void Revoked_session_is_not_usable()
    {
        var session = new DeviceSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddHours(1));

        session.Revoke();

        Assert.False(session.IsUsable(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Expired_session_is_not_usable()
    {
        var session = new DeviceSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(-1));

        Assert.False(session.IsUsable(DateTimeOffset.UtcNow));
    }
}
