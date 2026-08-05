using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Suppliers;

public sealed class Supplier : TenantEntity
{
    public Supplier(
        Guid id,
        Guid organisationId,
        string supplierNumber,
        string displayName,
        string? legalName,
        string? phone,
        string? email,
        string? taxNumber,
        SupplierStatus status = SupplierStatus.Active)
        : base(id, organisationId)
    {
        SupplierNumber = Guard.RequiredText(supplierNumber, nameof(supplierNumber), 32).ToUpperInvariant();
        DisplayName = Guard.RequiredText(displayName, nameof(displayName), 160);
        LegalName = Guard.OptionalText(legalName, nameof(legalName), 200);
        Phone = Guard.OptionalText(phone, nameof(phone), 40);
        Email = Guard.OptionalText(email, nameof(email), 254);
        TaxNumber = Guard.OptionalText(taxNumber, nameof(taxNumber), 80);
        Status = status;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public string SupplierNumber { get; }

    public string DisplayName { get; }

    public string? LegalName { get; }

    public string? Phone { get; }

    public string? Email { get; }

    public string? TaxNumber { get; }

    public SupplierStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public bool IsActive => Status == SupplierStatus.Active;

    public void PutOnHold()
    {
        Status = SupplierStatus.OnHold;
    }

    public void Deactivate()
    {
        Status = SupplierStatus.Inactive;
    }
}
