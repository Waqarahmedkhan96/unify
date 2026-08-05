using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Access;

public sealed class Permission : Entity
{
    public Permission(Guid id, string key, string description)
        : base(id)
    {
        Key = Guard.RequiredText(key, nameof(key), 120).ToLowerInvariant();
        Description = Guard.RequiredText(description, nameof(description), 240);
    }

    public string Key { get; }

    public string Description { get; }
}
