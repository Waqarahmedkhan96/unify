using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Accounting;

public sealed class FiscalPeriod : TenantEntity
{
    public FiscalPeriod(Guid id, Guid organisationId, string name, DateOnly startsOn, DateOnly endsOn, FiscalPeriodStatus status = FiscalPeriodStatus.Open)
        : base(id, organisationId)
    {
        if (endsOn < startsOn)
        {
            throw new ArgumentException("Fiscal period end date cannot be before start date.", nameof(endsOn));
        }

        Name = Guard.RequiredText(name, nameof(name), 80);
        StartsOn = startsOn;
        EndsOn = endsOn;
        Status = status;
    }

    public string Name { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
    public FiscalPeriodStatus Status { get; private set; }
    public bool IsOpen => Status == FiscalPeriodStatus.Open;
    public bool Contains(DateOnly date) => date >= StartsOn && date <= EndsOn;
    public void Close() => Status = FiscalPeriodStatus.Closed;
}
