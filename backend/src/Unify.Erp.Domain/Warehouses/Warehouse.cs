using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Warehouses;

public sealed class Warehouse : TenantEntity
{
    public Warehouse(
        Guid id,
        Guid organisationId,
        Guid branchId,
        string code,
        string name,
        WarehouseStatus status = WarehouseStatus.Active)
        : base(id, organisationId)
    {
        BranchId = Guard.RequiredId(branchId, nameof(branchId));
        Code = Guard.RequiredText(code, nameof(code), 32).ToUpperInvariant();
        Name = Guard.RequiredText(name, nameof(name), 120);
        Status = status;
    }

    public Guid BranchId { get; }

    public string Code { get; }

    public string Name { get; }

    public WarehouseStatus Status { get; private set; }

    public bool IsActive => Status == WarehouseStatus.Active;

    public void Disable()
    {
        Status = WarehouseStatus.Disabled;
    }
}
