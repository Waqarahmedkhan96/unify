namespace Unify.Erp.Contracts.Accounting;

public sealed record JournalEntryResponse(
    Guid Id,
    Guid OrganisationId,
    Guid FiscalPeriodId,
    string JournalNumber,
    DateOnly JournalDate,
    string Description,
    string Status,
    IReadOnlyCollection<JournalLineResponse> Lines);
