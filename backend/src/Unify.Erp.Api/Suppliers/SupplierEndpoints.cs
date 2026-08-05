using Unify.Erp.Api.Common;
using Unify.Erp.Application.Suppliers;
using Unify.Erp.Contracts.Auth;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Suppliers;

namespace Unify.Erp.Api.Suppliers;

public static class SupplierEndpoints
{
    public static IEndpointRouteBuilder MapSupplierEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/suppliers")
            .RequireAuthorization()
            .RequireAuthorization(PermissionNames.SuppliersManage)
            .WithTags("Suppliers");

        group.MapPost("/", CreateSupplierAsync)
            .WithName("CreateSupplier");

        group.MapGet("/", ListSuppliersAsync)
            .WithName("ListSuppliers");

        group.MapGet("/{supplierId:guid}", GetSupplierAsync)
            .WithName("GetSupplier");

        group.MapPost("/{supplierId:guid}/deactivate", DeactivateSupplierAsync)
            .WithName("DeactivateSupplier");

        return endpoints;
    }

    private static async Task<IResult> CreateSupplierAsync(
        CreateSupplierRequest request,
        HttpContext httpContext,
        ISupplierService supplierService,
        CancellationToken cancellationToken)
    {
        var validationResult = request.Validate();
        if (!validationResult.IsValid)
        {
            return validationResult.ToProblem(httpContext);
        }

        try
        {
            var response = await supplierService.CreateAsync(request, cancellationToken);

            return Results.Created($"/api/v1/suppliers/{response.Id}", response);
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { code = "suppliers.invalid_supplier", field = exception.ParamName });
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { code = "suppliers.invalid_supplier", message = exception.Message });
        }
    }

    private static async Task<IResult> ListSuppliersAsync(
        Guid organisationId,
        string? search,
        int? pageNumber,
        int? pageSize,
        ISupplierService supplierService,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "suppliers.organisation_required" });
        }

        var response = await supplierService.ListAsync(
            new ListSuppliersRequest(
                organisationId,
                search,
                new PagedRequest(pageNumber ?? 1, pageSize ?? 50)),
            cancellationToken);

        return Results.Ok(response);
    }

    private static async Task<IResult> GetSupplierAsync(
        Guid supplierId,
        Guid organisationId,
        ISupplierService supplierService,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "suppliers.organisation_required" });
        }

        var response = await supplierService.GetAsync(organisationId, supplierId, cancellationToken);

        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<IResult> DeactivateSupplierAsync(
        Guid supplierId,
        Guid organisationId,
        ISupplierService supplierService,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "suppliers.organisation_required" });
        }

        var deactivated = await supplierService.DeactivateAsync(organisationId, supplierId, cancellationToken);

        return deactivated ? Results.NoContent() : Results.NotFound();
    }
}
