namespace Unify.Erp.Contracts.Customers;

public sealed record CreateCustomerRequest(
    Guid OrganisationId,
    Guid BranchId,
    string CustomerNumber,
    string DisplayName,
    string? LegalName,
    string? Phone,
    string? Email,
    string? TaxNumber,
    decimal CreditLimit);
