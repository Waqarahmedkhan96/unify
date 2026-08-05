namespace Unify.Erp.Contracts.Payments;

public sealed record CustomerLedgerEntryResponse(
    Guid Id,
    Guid OrganisationId,
    Guid CustomerId,
    string EntryType,
    string ReferenceType,
    Guid ReferenceId,
    decimal Debit,
    decimal Credit,
    decimal BalanceImpact,
    DateTimeOffset EntryDateUtc);
