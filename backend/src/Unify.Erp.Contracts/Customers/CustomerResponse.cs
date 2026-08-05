namespace Unify.Erp.Contracts.Customers;

public sealed record CustomerResponse(
    Guid Id,
    Guid OrganisationId,
    Guid BranchId,
    string CustomerNumber,
    string DisplayName,
    string? LegalName,
    string? Phone,
    string? Email,
    string? TaxNumber,
    decimal CreditLimit,
    string Status,
    DateTimeOffset CreatedAtUtc);
