using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Finance;

public sealed class CustomerPayment : TenantEntity
{
    public CustomerPayment(
        Guid id,
        Guid organisationId,
        Guid branchId,
        Guid customerId,
        string receiptNumber,
        decimal amount,
        PaymentMethod method,
        DateTimeOffset paymentDateUtc,
        string? notes)
        : base(id, organisationId)
    {
        BranchId = Guard.RequiredId(branchId, nameof(branchId));
        CustomerId = Guard.RequiredId(customerId, nameof(customerId));
        ReceiptNumber = Guard.RequiredText(receiptNumber, nameof(receiptNumber), 40).ToUpperInvariant();
        Amount = Guard.PositiveQuantity(amount, nameof(amount));
        Method = method;
        PaymentDateUtc = paymentDateUtc;
        Notes = Guard.OptionalText(notes, nameof(notes), 500);
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid BranchId { get; }

    public Guid CustomerId { get; }

    public string ReceiptNumber { get; }

    public decimal Amount { get; }

    public PaymentMethod Method { get; }

    public DateTimeOffset PaymentDateUtc { get; }

    public string? Notes { get; }

    public DateTimeOffset CreatedAtUtc { get; }
}
