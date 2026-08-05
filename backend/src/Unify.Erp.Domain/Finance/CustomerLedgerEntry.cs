using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Finance;

public sealed class CustomerLedgerEntry : TenantEntity
{
    public CustomerLedgerEntry(
        Guid id,
        Guid organisationId,
        Guid customerId,
        CustomerLedgerEntryType entryType,
        string referenceType,
        Guid referenceId,
        decimal debit,
        decimal credit,
        DateTimeOffset entryDateUtc)
        : base(id, organisationId)
    {
        CustomerId = Guard.RequiredId(customerId, nameof(customerId));
        EntryType = entryType;
        ReferenceType = Guard.RequiredText(referenceType, nameof(referenceType), 60);
        ReferenceId = Guard.RequiredId(referenceId, nameof(referenceId));
        Debit = Guard.NonNegativeMoney(debit, nameof(debit));
        Credit = Guard.NonNegativeMoney(credit, nameof(credit));
        if (Debit == 0 && Credit == 0)
        {
            throw new ArgumentException("Either debit or credit is required.", nameof(debit));
        }

        EntryDateUtc = entryDateUtc;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid CustomerId { get; }

    public CustomerLedgerEntryType EntryType { get; }

    public string ReferenceType { get; }

    public Guid ReferenceId { get; }

    public decimal Debit { get; }

    public decimal Credit { get; }

    public DateTimeOffset EntryDateUtc { get; }

    public DateTimeOffset CreatedAtUtc { get; }

    public decimal BalanceImpact => Debit - Credit;
}
