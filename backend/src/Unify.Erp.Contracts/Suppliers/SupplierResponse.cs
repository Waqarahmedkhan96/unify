namespace Unify.Erp.Contracts.Suppliers;

public sealed record SupplierResponse(
    Guid Id,
    Guid OrganisationId,
    string SupplierNumber,
    string DisplayName,
    string? LegalName,
    string? Phone,
    string? Email,
    string? TaxNumber,
    string Status,
    DateTimeOffset CreatedAtUtc);
