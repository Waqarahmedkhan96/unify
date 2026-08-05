using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Inventory;

namespace Unify.Erp.Application.Inventory;

public interface IInventoryService
{
    Task<StockMovementResponse> AdjustAsync(CreateStockAdjustmentRequest request, CancellationToken cancellationToken);

    Task<StockTransferResponse> TransferAsync(CreateStockTransferRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<StockBalanceResponse>> ListBalancesAsync(Guid organisationId, Guid? warehouseId, CancellationToken cancellationToken);

    Task<PagedResponse<StockMovementResponse>> ListMovementsAsync(ListStockMovementsRequest request, CancellationToken cancellationToken);
}
