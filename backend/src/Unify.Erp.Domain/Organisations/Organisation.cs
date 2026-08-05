using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Organisations;

public sealed class Organisation : Entity
{
    public Organisation(
        Guid id,
        string legalName,
        string displayName,
        string baseCurrency,
        string timezone,
        OrganisationStatus status = OrganisationStatus.Active)
        : base(id)
    {
        LegalName = Guard.RequiredText(legalName, nameof(legalName), 200);
        DisplayName = Guard.RequiredText(displayName, nameof(displayName), 120);
        BaseCurrency = Guard.RequiredText(baseCurrency, nameof(baseCurrency), 3).ToUpperInvariant();
        Timezone = Guard.RequiredText(timezone, nameof(timezone), 100);
        Status = status;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public string LegalName { get; }

    public string DisplayName { get; }

    public string BaseCurrency { get; }

    public string Timezone { get; }

    public OrganisationStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public bool IsActive => Status == OrganisationStatus.Active;

    public void Suspend()
    {
        Status = OrganisationStatus.Suspended;
    }

    public void Deactivate()
    {
        Status = OrganisationStatus.Deactivated;
    }

}
