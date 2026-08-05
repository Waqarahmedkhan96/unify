using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Suppliers;

namespace Unify.Erp.Application.Suppliers;

public interface ISupplierService
{
    Task<SupplierResponse> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken);

    Task<SupplierResponse?> GetAsync(Guid organisationId, Guid supplierId, CancellationToken cancellationToken);

    Task<PagedResponse<SupplierResponse>> ListAsync(ListSuppliersRequest request, CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(Guid organisationId, Guid supplierId, CancellationToken cancellationToken);
}
