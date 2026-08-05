namespace Unify.Erp.Contracts.Accounting;

public sealed record CreateAccountRequest(Guid OrganisationId, string Code, string Name, string Type);
