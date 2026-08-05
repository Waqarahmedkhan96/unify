using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Accounting;

public sealed class JournalEntry : TenantEntity
{
    public JournalEntry(
        Guid id,
        Guid organisationId,
        Guid fiscalPeriodId,
        string journalNumber,
        DateOnly journalDate,
        string description,
        JournalStatus status = JournalStatus.Posted)
        : base(id, organisationId)
    {
        FiscalPeriodId = Guard.RequiredId(fiscalPeriodId, nameof(fiscalPeriodId));
        JournalNumber = Guard.RequiredText(journalNumber, nameof(journalNumber), 40).ToUpperInvariant();
        JournalDate = journalDate;
        Description = Guard.RequiredText(description, nameof(description), 240);
        Status = status;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid FiscalPeriodId { get; }
    public string JournalNumber { get; }
    public DateOnly JournalDate { get; }
    public string Description { get; }
    public JournalStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
}
