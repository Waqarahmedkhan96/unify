namespace Unify.Erp.Contracts.Accounting;

public sealed record JournalLineResponse(Guid Id, Guid AccountId, string Description, decimal Debit, decimal Credit);
