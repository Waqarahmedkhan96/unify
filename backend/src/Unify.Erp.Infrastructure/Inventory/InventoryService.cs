using Microsoft.EntityFrameworkCore;
using Unify.Erp.Application.Inventory;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Inventory;
using Unify.Erp.Domain.Inventory;
using Unify.Erp.Domain.Products;
using Unify.Erp.Domain.Warehouses;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Inventory;

public sealed class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _dbContext;

    public InventoryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StockMovementResponse> AdjustAsync(
        CreateStockAdjustmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<StockMovementType>(request.MovementType, ignoreCase: true, out var movementType)
            || movementType is not (StockMovementType.AdjustmentIn or StockMovementType.AdjustmentOut))
        {
            throw new InvalidOperationException("Adjustment movement type is invalid.");
        }

        var warehouse = await GetWarehouseAsync(request.OrganisationId, request.WarehouseId, cancellationToken);
        await EnsureTrackedProductAsync(request.OrganisationId, request.ProductId, cancellationToken);

        var movement = new StockMovement(
            Guid.NewGuid(),
            request.OrganisationId,
            warehouse.BranchId,
            request.WarehouseId,
            request.ProductId,
            movementType,
            request.Quantity,
            "StockAdjustment",
            null,
            request.Notes,
            DateTimeOffset.UtcNow);

        await ApplyMovementAsync(movement, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(movement);
    }

    public async Task<StockTransferResponse> TransferAsync(
        CreateStockTransferRequest request,
        CancellationToken cancellationToken)
    {
        if (request.SourceWarehouseId == request.DestinationWarehouseId)
        {
            throw new InvalidOperationException("Source and destination warehouses must be different.");
        }

        var sourceWarehouse = await GetWarehouseAsync(request.OrganisationId, request.SourceWarehouseId, cancellationToken);
        var destinationWarehouse = await GetWarehouseAsync(request.OrganisationId, request.DestinationWarehouseId, cancellationToken);
        await EnsureTrackedProductAsync(request.OrganisationId, request.ProductId, cancellationToken);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var transferId = Guid.NewGuid();
        var occurredAtUtc = DateTimeOffset.UtcNow;
        var sourceMovement = new StockMovement(
            Guid.NewGuid(),
            request.OrganisationId,
            sourceWarehouse.BranchId,
            request.SourceWarehouseId,
            request.ProductId,
            StockMovementType.TransferOut,
            request.Quantity,
            "StockTransfer",
            transferId,
            request.Notes,
            occurredAtUtc);
        var destinationMovement = new StockMovement(
            Guid.NewGuid(),
            request.OrganisationId,
            destinationWarehouse.BranchId,
            request.DestinationWarehouseId,
            request.ProductId,
            StockMovementType.TransferIn,
            request.Quantity,
            "StockTransfer",
            transferId,
            request.Notes,
            occurredAtUtc);

        await ApplyMovementAsync(sourceMovement, cancellationToken);
        await ApplyMovementAsync(destinationMovement, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new StockTransferResponse(transferId, ToResponse(sourceMovement), ToResponse(destinationMovement));
    }

    public async Task<IReadOnlyCollection<StockBalanceResponse>> ListBalancesAsync(
        Guid organisationId,
        Guid? warehouseId,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.StockBalances
            .AsNoTracking()
            .Where(balance => balance.OrganisationId == organisationId);

        if (warehouseId.HasValue)
        {
            query = query.Where(balance => balance.WarehouseId == warehouseId.Value);
        }

        return await query
            .OrderBy(balance => balance.WarehouseId)
            .ThenBy(balance => balance.ProductId)
            .Select(balance => ToResponse(balance))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResponse<StockMovementResponse>> ListMovementsAsync(
        ListStockMovementsRequest request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.Page.NormalizedPageNumber;
        var pageSize = request.Page.NormalizedPageSize;
        var query = _dbContext.StockMovements
            .AsNoTracking()
            .Where(movement => movement.OrganisationId == request.OrganisationId);

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(movement => movement.WarehouseId == request.WarehouseId.Value);
        }

        if (request.ProductId.HasValue)
        {
            query = query.Where(movement => movement.ProductId == request.ProductId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(movement => movement.OccurredAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(movement => ToResponse(movement))
            .ToListAsync(cancellationToken);

        return new PagedResponse<StockMovementResponse>(items, pageNumber, pageSize, totalCount);
    }

    private async Task ApplyMovementAsync(StockMovement movement, CancellationToken cancellationToken)
    {
        var balance = await _dbContext.StockBalances.SingleOrDefaultAsync(
            item => item.OrganisationId == movement.OrganisationId
                && item.WarehouseId == movement.WarehouseId
                && item.ProductId == movement.ProductId,
            cancellationToken);

        if (balance is null)
        {
            balance = new StockBalance(Guid.NewGuid(), movement.OrganisationId, movement.WarehouseId, movement.ProductId);
            _dbContext.StockBalances.Add(balance);
        }

        balance.Apply(movement.SignedQuantity);
        _dbContext.StockMovements.Add(movement);
    }

    private async Task<Warehouse> GetWarehouseAsync(Guid organisationId, Guid warehouseId, CancellationToken cancellationToken)
    {
        var warehouse = await _dbContext.Warehouses.SingleOrDefaultAsync(
            item => item.OrganisationId == organisationId && item.Id == warehouseId,
            cancellationToken);

        return warehouse ?? throw new InvalidOperationException("Warehouse does not exist for the organisation.");
    }

    private async Task EnsureTrackedProductAsync(Guid organisationId, Guid productId, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.SingleOrDefaultAsync(
            item => item.OrganisationId == organisationId && item.Id == productId,
            cancellationToken);

        if (product is null)
        {
            throw new InvalidOperationException("Product does not exist for the organisation.");
        }

        if (!product.IsInventoryTracked || product.Status != ProductStatus.Active)
        {
            throw new InvalidOperationException("Product is not available for inventory movements.");
        }
    }

    private static StockBalanceResponse ToResponse(StockBalance balance)
    {
        return new StockBalanceResponse(
            balance.Id,
            balance.OrganisationId,
            balance.WarehouseId,
            balance.ProductId,
            balance.QuantityOnHand,
            balance.UpdatedAtUtc);
    }

    private static StockMovementResponse ToResponse(StockMovement movement)
    {
        return new StockMovementResponse(
            movement.Id,
            movement.OrganisationId,
            movement.BranchId,
            movement.WarehouseId,
            movement.ProductId,
            movement.MovementType.ToString(),
            movement.Quantity,
            movement.SignedQuantity,
            movement.ReferenceType,
            movement.ReferenceId,
            movement.Notes,
            movement.OccurredAtUtc);
    }
}
