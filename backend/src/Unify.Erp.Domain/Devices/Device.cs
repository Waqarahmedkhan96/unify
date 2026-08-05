using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Devices;

public sealed class Device : TenantEntity
{
    public Device(
        Guid id,
        Guid organisationId,
        string name,
        DeviceType type,
        DeviceStatus status = DeviceStatus.PendingApproval)
        : base(id, organisationId)
    {
        Name = Guard.RequiredText(name, nameof(name), 120);
        Type = type;
        Status = status;
    }

    public string Name { get; }

    public DeviceType Type { get; }

    public DeviceStatus Status { get; private set; }

    public bool CanSynchronize => Status == DeviceStatus.Approved;

    public void Approve()
    {
        Status = DeviceStatus.Approved;
    }

    public void Revoke()
    {
        Status = DeviceStatus.Revoked;
    }
}
