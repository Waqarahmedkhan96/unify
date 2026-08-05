using Unify.Erp.Api.Common;
using Unify.Erp.Application.Products;
using Unify.Erp.Contracts.Auth;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Products;

namespace Unify.Erp.Api.Products;

public static class ProductCatalogEndpoints
{
    public static IEndpointRouteBuilder MapProductCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/products")
            .RequireAuthorization()
            .RequireAuthorization(PermissionNames.ProductsManage)
            .WithTags("Products");

        group.MapPost("/units", CreateUnitAsync).WithName("CreateUnitOfMeasure");
        group.MapGet("/units", ListUnitsAsync).WithName("ListUnitsOfMeasure");
        group.MapPost("/categories", CreateCategoryAsync).WithName("CreateProductCategory");
        group.MapGet("/categories", ListCategoriesAsync).WithName("ListProductCategories");
        group.MapPost("/", CreateProductAsync).WithName("CreateProduct");
        group.MapGet("/", ListProductsAsync).WithName("ListProducts");
        group.MapGet("/{productId:guid}", GetProductAsync).WithName("GetProduct");
        group.MapPost("/{productId:guid}/deactivate", DeactivateProductAsync).WithName("DeactivateProduct");

        return endpoints;
    }

    private static async Task<IResult> CreateUnitAsync(
        CreateUnitOfMeasureRequest request,
        HttpContext httpContext,
        IProductCatalogService service,
        CancellationToken cancellationToken)
    {
        var validationResult = request.Validate();
        if (!validationResult.IsValid)
        {
            return validationResult.ToProblem(httpContext);
        }

        try
        {
            var response = await service.CreateUnitOfMeasureAsync(request, cancellationToken);
            return Results.Created($"/api/v1/products/units/{response.Id}", response);
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { code = "products.invalid_unit", message = exception.Message });
        }
    }

    private static async Task<IResult> ListUnitsAsync(
        Guid organisationId,
        IProductCatalogService service,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "products.organisation_required" });
        }

        return Results.Ok(await service.ListUnitsOfMeasureAsync(organisationId, cancellationToken));
    }

    private static async Task<IResult> CreateCategoryAsync(
        CreateProductCategoryRequest request,
        HttpContext httpContext,
        IProductCatalogService service,
        CancellationToken cancellationToken)
    {
        var validationResult = request.Validate();
        if (!validationResult.IsValid)
        {
            return validationResult.ToProblem(httpContext);
        }

        try
        {
            var response = await service.CreateCategoryAsync(request, cancellationToken);
            return Results.Created($"/api/v1/products/categories/{response.Id}", response);
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { code = "products.invalid_category", message = exception.Message });
        }
    }

    private static async Task<IResult> ListCategoriesAsync(
        Guid organisationId,
        IProductCatalogService service,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "products.organisation_required" });
        }

        return Results.Ok(await service.ListCategoriesAsync(organisationId, cancellationToken));
    }

    private static async Task<IResult> CreateProductAsync(
        CreateProductRequest request,
        HttpContext httpContext,
        IProductCatalogService service,
        CancellationToken cancellationToken)
    {
        var validationResult = request.Validate();
        if (!validationResult.IsValid)
        {
            return validationResult.ToProblem(httpContext);
        }

        try
        {
            var response = await service.CreateProductAsync(request, cancellationToken);
            return Results.Created($"/api/v1/products/{response.Id}", response);
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { code = "products.invalid_product", field = exception.ParamName });
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { code = "products.invalid_product", message = exception.Message });
        }
    }

    private static async Task<IResult> ListProductsAsync(
        Guid organisationId,
        Guid? categoryId,
        string? search,
        int? pageNumber,
        int? pageSize,
        IProductCatalogService service,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "products.organisation_required" });
        }

        var response = await service.ListProductsAsync(
            new ListProductsRequest(
                organisationId,
                categoryId,
                search,
                new PagedRequest(pageNumber ?? 1, pageSize ?? 50)),
            cancellationToken);

        return Results.Ok(response);
    }

    private static async Task<IResult> GetProductAsync(
        Guid productId,
        Guid organisationId,
        IProductCatalogService service,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "products.organisation_required" });
        }

        var response = await service.GetProductAsync(organisationId, productId, cancellationToken);

        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<IResult> DeactivateProductAsync(
        Guid productId,
        Guid organisationId,
        IProductCatalogService service,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "products.organisation_required" });
        }

        var deactivated = await service.DeactivateProductAsync(organisationId, productId, cancellationToken);

        return deactivated ? Results.NoContent() : Results.NotFound();
    }
}
