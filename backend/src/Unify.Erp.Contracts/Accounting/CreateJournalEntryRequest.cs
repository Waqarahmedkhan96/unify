namespace Unify.Erp.Contracts.Accounting;

public sealed record CreateJournalEntryRequest(
    Guid OrganisationId,
    string JournalNumber,
    DateOnly JournalDate,
    string Description,
    IReadOnlyCollection<CreateJournalLineRequest> Lines);
