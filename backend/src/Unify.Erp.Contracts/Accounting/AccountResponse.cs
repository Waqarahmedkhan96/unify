namespace Unify.Erp.Contracts.Accounting;

public sealed record AccountResponse(Guid Id, Guid OrganisationId, string Code, string Name, string Type, string Status);
