namespace Unify.Erp.Contracts.Accounting;

public sealed record CreateJournalLineRequest(Guid AccountId, string Description, decimal Debit, decimal Credit);
