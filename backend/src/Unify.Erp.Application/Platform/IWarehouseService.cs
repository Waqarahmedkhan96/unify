using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Platform;

namespace Unify.Erp.Application.Platform;

public interface IWarehouseService
{
    Task<WarehouseResponse> CreateAsync(CreateWarehouseRequest request, CancellationToken cancellationToken);

    Task<PagedResponse<WarehouseResponse>> ListByOrganisationAsync(
        Guid organisationId,
        PagedRequest request,
        CancellationToken cancellationToken);
}
