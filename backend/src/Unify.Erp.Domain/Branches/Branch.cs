using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Branches;

public sealed class Branch : TenantEntity
{
    public Branch(
        Guid id,
        Guid organisationId,
        string code,
        string name,
        string timezone,
        BranchStatus status = BranchStatus.Active)
        : base(id, organisationId)
    {
        Code = Guard.RequiredText(code, nameof(code), 32).ToUpperInvariant();
        Name = Guard.RequiredText(name, nameof(name), 120);
        Timezone = Guard.RequiredText(timezone, nameof(timezone), 100);
        Status = status;
    }

    public string Code { get; }

    public string Name { get; }

    public string Timezone { get; }

    public BranchStatus Status { get; private set; }

    public bool IsActive => Status == BranchStatus.Active;

    public void Disable()
    {
        Status = BranchStatus.Disabled;
    }
}
