using Unify.Erp.Contracts.Platform;

namespace Unify.Erp.Application.Platform;

public interface IWarehouseService
{
    Task<WarehouseResponse> CreateAsync(CreateWarehouseRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<WarehouseResponse>> ListByOrganisationAsync(Guid organisationId, CancellationToken cancellationToken);
}
