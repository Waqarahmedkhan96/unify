using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Sessions;

public sealed class DeviceSession : TenantEntity
{
    public DeviceSession(
        Guid id,
        Guid organisationId,
        Guid userId,
        Guid deviceId,
        DateTimeOffset expiresAtUtc,
        SessionStatus status = SessionStatus.Active)
        : base(id, organisationId)
    {
        UserId = Guard.RequiredId(userId, nameof(userId));
        DeviceId = Guard.RequiredId(deviceId, nameof(deviceId));
        ExpiresAtUtc = expiresAtUtc;
        Status = status;
    }

    public Guid UserId { get; }

    public Guid DeviceId { get; }

    public DateTimeOffset ExpiresAtUtc { get; }

    public SessionStatus Status { get; private set; }

    public bool IsUsable(DateTimeOffset nowUtc)
    {
        return Status == SessionStatus.Active && ExpiresAtUtc > nowUtc;
    }

    public void Revoke()
    {
        Status = SessionStatus.Revoked;
    }
}
