using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Access;

public sealed class Role : TenantEntity
{
    private readonly HashSet<string> _permissionKeys = [];

    public Role(Guid id, Guid organisationId, string name, RoleStatus status = RoleStatus.Active)
        : base(id, organisationId)
    {
        Name = Guard.RequiredText(name, nameof(name), 120);
        Status = status;
    }

    public string Name { get; }

    public RoleStatus Status { get; private set; }

    public IReadOnlyCollection<string> PermissionKeys => _permissionKeys;

    public void Grant(string permissionKey)
    {
        _permissionKeys.Add(Guard.RequiredText(permissionKey, nameof(permissionKey), 120).ToLowerInvariant());
    }

    public bool HasPermission(string permissionKey)
    {
        var normalized = Guard.RequiredText(permissionKey, nameof(permissionKey), 120).ToLowerInvariant();

        return _permissionKeys.Contains(normalized);
    }

    public void Disable()
    {
        Status = RoleStatus.Disabled;
    }
}
