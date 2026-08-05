using Unify.Erp.Contracts.Payments;

namespace Unify.Erp.Application.Payments;

public interface ICustomerPaymentService
{
    Task<CustomerPaymentResponse> CreateAsync(CreateCustomerPaymentRequest request, CancellationToken cancellationToken);

    Task<CustomerBalanceResponse> GetBalanceAsync(Guid organisationId, Guid customerId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CustomerLedgerEntryResponse>> ListLedgerAsync(
        Guid organisationId,
        Guid customerId,
        CancellationToken cancellationToken);
}
