namespace Unify.Erp.Contracts.Accounting;

public sealed record FiscalPeriodResponse(Guid Id, Guid OrganisationId, string Name, DateOnly StartsOn, DateOnly EndsOn, string Status);
