using Unify.Erp.Api.Common;
using Unify.Erp.Application.Inventory;
using Unify.Erp.Contracts.Auth;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Inventory;

namespace Unify.Erp.Api.Inventory;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/inventory")
            .RequireAuthorization()
            .RequireAuthorization(PermissionNames.InventoryManage)
            .WithTags("Inventory");

        group.MapPost("/adjustments", CreateAdjustmentAsync).WithName("CreateStockAdjustment");
        group.MapPost("/transfers", CreateTransferAsync).WithName("CreateStockTransfer");
        group.MapGet("/balances", ListBalancesAsync).WithName("ListStockBalances");
        group.MapGet("/movements", ListMovementsAsync).WithName("ListStockMovements");

        return endpoints;
    }

    private static async Task<IResult> CreateAdjustmentAsync(
        CreateStockAdjustmentRequest request,
        HttpContext httpContext,
        IInventoryService service,
        CancellationToken cancellationToken)
    {
        var validationResult = request.Validate();
        if (!validationResult.IsValid)
        {
            return validationResult.ToProblem(httpContext);
        }

        try
        {
            return Results.Created("/api/v1/inventory/movements", await service.AdjustAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { code = "inventory.invalid_adjustment", field = exception.ParamName });
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { code = "inventory.invalid_adjustment", message = exception.Message });
        }
    }

    private static async Task<IResult> CreateTransferAsync(
        CreateStockTransferRequest request,
        HttpContext httpContext,
        IInventoryService service,
        CancellationToken cancellationToken)
    {
        var validationResult = request.Validate();
        if (!validationResult.IsValid)
        {
            return validationResult.ToProblem(httpContext);
        }

        try
        {
            return Results.Created("/api/v1/inventory/transfers", await service.TransferAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { code = "inventory.invalid_transfer", field = exception.ParamName });
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { code = "inventory.invalid_transfer", message = exception.Message });
        }
    }

    private static async Task<IResult> ListBalancesAsync(
        Guid organisationId,
        Guid? warehouseId,
        IInventoryService service,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "inventory.organisation_required" });
        }

        return Results.Ok(await service.ListBalancesAsync(organisationId, warehouseId, cancellationToken));
    }

    private static async Task<IResult> ListMovementsAsync(
        Guid organisationId,
        Guid? warehouseId,
        Guid? productId,
        int? pageNumber,
        int? pageSize,
        IInventoryService service,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "inventory.organisation_required" });
        }

        return Results.Ok(await service.ListMovementsAsync(
            new ListStockMovementsRequest(
                organisationId,
                warehouseId,
                productId,
                new PagedRequest(pageNumber ?? 1, pageSize ?? 50)),
            cancellationToken));
    }
}
