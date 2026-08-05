namespace Unify.Erp.Contracts.Payments;

public sealed record CreateCustomerPaymentRequest(
    Guid OrganisationId,
    Guid BranchId,
    Guid CustomerId,
    string ReceiptNumber,
    decimal Amount,
    string Method,
    DateTimeOffset PaymentDateUtc,
    string? Notes,
    IReadOnlyCollection<PaymentAllocationRequest> Allocations);
