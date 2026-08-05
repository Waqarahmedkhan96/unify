namespace Unify.Erp.Contracts.Accounting;

public sealed record CreateFiscalPeriodRequest(Guid OrganisationId, string Name, DateOnly StartsOn, DateOnly EndsOn);
