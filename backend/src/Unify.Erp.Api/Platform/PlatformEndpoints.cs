using Unify.Erp.Api.Common;
using Unify.Erp.Application.Audit;
using Unify.Erp.Application.Platform;
using Unify.Erp.Contracts.Audit;
using Unify.Erp.Contracts.Auth;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Platform;

namespace Unify.Erp.Api.Platform;

public static class PlatformEndpoints
{
    public static IEndpointRouteBuilder MapPlatformEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/platform")
            .RequireAuthorization()
            .RequireAuthorization(PermissionNames.PlatformManage)
            .WithTags("Platform");

        group.MapPost("/organisations", CreateOrganisationAsync)
            .WithName("CreateOrganisation");

        group.MapGet("/organisations", ListOrganisationsAsync)
            .WithName("ListOrganisations");

        group.MapPost("/branches", CreateBranchAsync)
            .WithName("CreateBranch");

        group.MapGet("/organisations/{organisationId:guid}/branches", ListBranchesAsync)
            .WithName("ListBranches");

        group.MapPost("/warehouses", CreateWarehouseAsync)
            .WithName("CreateWarehouse");

        group.MapGet("/organisations/{organisationId:guid}/warehouses", ListWarehousesAsync)
            .WithName("ListWarehouses");

        group.MapGet("/audit-entries", ListAuditEntriesAsync)
            .WithName("ListAuditEntries");

        return endpoints;
    }

    private static async Task<IResult> CreateOrganisationAsync(
        CreateOrganisationRequest request,
        HttpContext httpContext,
        IOrganisationService organisationService,
        CancellationToken cancellationToken)
    {
        var validationResult = request.Validate();
        if (!validationResult.IsValid)
        {
            return validationResult.ToProblem(httpContext);
        }

        try
        {
            var response = await organisationService.CreateAsync(request, cancellationToken);

            return Results.Created($"/api/v1/platform/organisations/{response.Id}", response);
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { code = "platform.invalid_organisation", field = exception.ParamName });
        }
    }

    private static async Task<IResult> ListOrganisationsAsync(
        int? pageNumber,
        int? pageSize,
        IOrganisationService organisationService,
        CancellationToken cancellationToken)
    {
        var response = await organisationService.ListAsync(
            new PagedRequest(pageNumber ?? 1, pageSize ?? 50),
            cancellationToken);

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateBranchAsync(
        CreateBranchRequest request,
        HttpContext httpContext,
        IBranchService branchService,
        CancellationToken cancellationToken)
    {
        var validationResult = request.Validate();
        if (!validationResult.IsValid)
        {
            return validationResult.ToProblem(httpContext);
        }

        try
        {
            var response = await branchService.CreateAsync(request, cancellationToken);

            return Results.Created(
                $"/api/v1/platform/organisations/{response.OrganisationId}/branches/{response.Id}",
                response);
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { code = "platform.invalid_branch", field = exception.ParamName });
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { code = "platform.invalid_branch", message = exception.Message });
        }
    }

    private static async Task<IResult> ListBranchesAsync(
        Guid organisationId,
        int? pageNumber,
        int? pageSize,
        IBranchService branchService,
        CancellationToken cancellationToken)
    {
        var response = await branchService.ListByOrganisationAsync(
            organisationId,
            new PagedRequest(pageNumber ?? 1, pageSize ?? 50),
            cancellationToken);

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateWarehouseAsync(
        CreateWarehouseRequest request,
        HttpContext httpContext,
        IWarehouseService warehouseService,
        CancellationToken cancellationToken)
    {
        var validationResult = request.Validate();
        if (!validationResult.IsValid)
        {
            return validationResult.ToProblem(httpContext);
        }

        try
        {
            var response = await warehouseService.CreateAsync(request, cancellationToken);

            return Results.Created(
                $"/api/v1/platform/organisations/{response.OrganisationId}/warehouses/{response.Id}",
                response);
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { code = "platform.invalid_warehouse", field = exception.ParamName });
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { code = "platform.invalid_warehouse", message = exception.Message });
        }
    }

    private static async Task<IResult> ListWarehousesAsync(
        Guid organisationId,
        int? pageNumber,
        int? pageSize,
        IWarehouseService warehouseService,
        CancellationToken cancellationToken)
    {
        var response = await warehouseService.ListByOrganisationAsync(
            organisationId,
            new PagedRequest(pageNumber ?? 1, pageSize ?? 50),
            cancellationToken);

        return Results.Ok(response);
    }

    private static async Task<IResult> ListAuditEntriesAsync(
        Guid? organisationId,
        string? entityName,
        string? entityId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int? pageNumber,
        int? pageSize,
        IAuditService auditService,
        CancellationToken cancellationToken)
    {
        var response = await auditService.ListAsync(
            new ListAuditEntriesRequest(
                organisationId,
                entityName,
                entityId,
                fromUtc,
                toUtc,
                new PagedRequest(pageNumber ?? 1, pageSize ?? 50)),
            cancellationToken);

        return Results.Ok(response);
    }
}
