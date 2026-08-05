using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Finance;

public sealed class PaymentAllocation : TenantEntity
{
    public PaymentAllocation(Guid id, Guid organisationId, Guid paymentId, Guid saleId, decimal amount)
        : base(id, organisationId)
    {
        PaymentId = Guard.RequiredId(paymentId, nameof(paymentId));
        SaleId = Guard.RequiredId(saleId, nameof(saleId));
        Amount = Guard.PositiveQuantity(amount, nameof(amount));
    }

    public Guid PaymentId { get; }

    public Guid SaleId { get; }

    public decimal Amount { get; }
}
