using Microsoft.EntityFrameworkCore;
using Unify.Erp.Application.Platform;
using Unify.Erp.Contracts.Platform;
using Unify.Erp.Domain.Warehouses;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Platform;

public sealed class WarehouseService : IWarehouseService
{
    private readonly ApplicationDbContext _dbContext;

    public WarehouseService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WarehouseResponse> CreateAsync(
        CreateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        var branchExists = await _dbContext.Branches
            .AnyAsync(
                branch => branch.Id == request.BranchId && branch.OrganisationId == request.OrganisationId,
                cancellationToken);

        if (!branchExists)
        {
            throw new InvalidOperationException("Branch does not exist for the organisation.");
        }

        var warehouse = new Warehouse(
            Guid.NewGuid(),
            request.OrganisationId,
            request.BranchId,
            request.Code,
            request.Name);

        _dbContext.Warehouses.Add(warehouse);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(warehouse);
    }

    public async Task<IReadOnlyCollection<WarehouseResponse>> ListByOrganisationAsync(
        Guid organisationId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Warehouses
            .AsNoTracking()
            .Where(warehouse => warehouse.OrganisationId == organisationId)
            .OrderBy(warehouse => warehouse.Code)
            .Select(warehouse => ToResponse(warehouse))
            .ToListAsync(cancellationToken);
    }

    private static WarehouseResponse ToResponse(Warehouse warehouse)
    {
        return new WarehouseResponse(
            warehouse.Id,
            warehouse.OrganisationId,
            warehouse.BranchId,
            warehouse.Code,
            warehouse.Name,
            warehouse.Status.ToString());
    }
}
