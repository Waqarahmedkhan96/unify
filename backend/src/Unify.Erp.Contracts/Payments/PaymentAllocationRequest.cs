namespace Unify.Erp.Contracts.Payments;

public sealed record PaymentAllocationRequest(Guid SaleId, decimal Amount);
