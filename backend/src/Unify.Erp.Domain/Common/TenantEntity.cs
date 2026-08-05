namespace Unify.Erp.Domain.Common;

public abstract class TenantEntity : Entity
{
    protected TenantEntity(Guid id, Guid organisationId)
        : base(id)
    {
        if (organisationId == Guid.Empty)
        {
            throw new ArgumentException("Organisation id cannot be empty.", nameof(organisationId));
        }

        OrganisationId = organisationId;
    }

    public Guid OrganisationId { get; }
}
