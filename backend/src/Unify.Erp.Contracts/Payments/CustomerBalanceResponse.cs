namespace Unify.Erp.Contracts.Payments;

public sealed record CustomerBalanceResponse(Guid OrganisationId, Guid CustomerId, decimal Balance);
