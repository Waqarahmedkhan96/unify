namespace Unify.Erp.Contracts.Payments;

public sealed record CustomerPaymentResponse(
    Guid Id,
    Guid OrganisationId,
    Guid BranchId,
    Guid CustomerId,
    string ReceiptNumber,
    decimal Amount,
    string Method,
    DateTimeOffset PaymentDateUtc,
    string? Notes);
