using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Accounting;

public sealed class JournalLine : TenantEntity
{
    public JournalLine(Guid id, Guid organisationId, Guid journalEntryId, Guid accountId, string description, decimal debit, decimal credit)
        : base(id, organisationId)
    {
        JournalEntryId = Guard.RequiredId(journalEntryId, nameof(journalEntryId));
        AccountId = Guard.RequiredId(accountId, nameof(accountId));
        Description = Guard.RequiredText(description, nameof(description), 160);
        Debit = Guard.NonNegativeMoney(debit, nameof(debit));
        Credit = Guard.NonNegativeMoney(credit, nameof(credit));
        if ((Debit == 0 && Credit == 0) || (Debit > 0 && Credit > 0))
        {
            throw new ArgumentException("Journal line must contain either debit or credit.", nameof(debit));
        }
    }

    public Guid JournalEntryId { get; }
    public Guid AccountId { get; }
    public string Description { get; }
    public decimal Debit { get; }
    public decimal Credit { get; }
}
