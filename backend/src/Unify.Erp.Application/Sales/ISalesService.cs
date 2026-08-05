using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Sales;

namespace Unify.Erp.Application.Sales;

public interface ISalesService
{
    Task<SaleResponse> CreateAsync(CreateSaleRequest request, CancellationToken cancellationToken);

    Task<SaleResponse?> GetAsync(Guid organisationId, Guid saleId, CancellationToken cancellationToken);

    Task<PagedResponse<SaleResponse>> ListAsync(ListSalesRequest request, CancellationToken cancellationToken);
}
