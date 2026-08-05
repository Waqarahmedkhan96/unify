using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Customers;

namespace Unify.Erp.Application.Customers;

public interface ICustomerService
{
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken);

    Task<CustomerResponse?> GetAsync(Guid organisationId, Guid customerId, CancellationToken cancellationToken);

    Task<PagedResponse<CustomerResponse>> ListAsync(ListCustomersRequest request, CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(Guid organisationId, Guid customerId, CancellationToken cancellationToken);
}
