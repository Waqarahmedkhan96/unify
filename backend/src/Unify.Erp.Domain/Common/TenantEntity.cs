namespace Unify.Erp.Domain.Common;

public abstract class TenantEntity : Entity
{
    protected TenantEntity(Guid id, Guid organisationId)
        : base(id)
    {
        OrganisationId = Guard.RequiredId(organisationId, nameof(organisationId));
    }

    public Guid OrganisationId { get; }
}
