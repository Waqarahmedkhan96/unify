namespace Unify.Erp.Contracts.Suppliers;

public sealed record CreateSupplierRequest(
    Guid OrganisationId,
    string SupplierNumber,
    string DisplayName,
    string? LegalName,
    string? Phone,
    string? Email,
    string? TaxNumber);
