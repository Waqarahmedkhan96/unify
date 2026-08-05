namespace Unify.Erp.Domain.Common;

public abstract class Entity
{
    protected Entity(Guid id)
    {
        Id = Guard.RequiredId(id, nameof(id));
    }

    public Guid Id { get; }
}
