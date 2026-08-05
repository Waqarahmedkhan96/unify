using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Customers;

public sealed class Customer : TenantEntity
{
    public Customer(
        Guid id,
        Guid organisationId,
        Guid branchId,
        string customerNumber,
        string displayName,
        string? legalName,
        string? phone,
        string? email,
        string? taxNumber,
        decimal creditLimit,
        CustomerStatus status = CustomerStatus.Active)
        : base(id, organisationId)
    {
        BranchId = Guard.RequiredId(branchId, nameof(branchId));
        CustomerNumber = Guard.RequiredText(customerNumber, nameof(customerNumber), 32).ToUpperInvariant();
        DisplayName = Guard.RequiredText(displayName, nameof(displayName), 160);
        LegalName = Guard.OptionalText(legalName, nameof(legalName), 200);
        Phone = Guard.OptionalText(phone, nameof(phone), 40);
        Email = Guard.OptionalText(email, nameof(email), 254);
        TaxNumber = Guard.OptionalText(taxNumber, nameof(taxNumber), 80);
        CreditLimit = Guard.NonNegativeMoney(creditLimit, nameof(creditLimit));
        Status = status;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid BranchId { get; }

    public string CustomerNumber { get; }

    public string DisplayName { get; }

    public string? LegalName { get; }

    public string? Phone { get; }

    public string? Email { get; }

    public string? TaxNumber { get; }

    public decimal CreditLimit { get; }

    public CustomerStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public bool IsActive => Status == CustomerStatus.Active;

    public void PutOnHold()
    {
        Status = CustomerStatus.OnHold;
    }

    public void Deactivate()
    {
        Status = CustomerStatus.Inactive;
    }
}
