using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Accounting;

public sealed class Account : TenantEntity
{
    public Account(Guid id, Guid organisationId, string code, string name, AccountType type, AccountStatus status = AccountStatus.Active)
        : base(id, organisationId)
    {
        Code = Guard.RequiredText(code, nameof(code), 32).ToUpperInvariant();
        Name = Guard.RequiredText(name, nameof(name), 120);
        Type = type;
        Status = status;
    }

    public string Code { get; }
    public string Name { get; }
    public AccountType Type { get; }
    public AccountStatus Status { get; private set; }
    public bool IsActive => Status == AccountStatus.Active;
}
