using Unify.Erp.Api.Common;
using Unify.Erp.Application.Purchasing;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Purchasing;

namespace Unify.Erp.Api.Purchasing;

public static class PurchasingEndpoints
{
    public static IEndpointRouteBuilder MapPurchasingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/purchasing")
            .RequireAuthorization()
            .WithTags("Purchasing");

        group.MapPost("/orders", CreatePurchaseOrderAsync).WithName("CreatePurchaseOrder");
        group.MapGet("/orders", ListPurchaseOrdersAsync).WithName("ListPurchaseOrders");
        group.MapPost("/goods-receipts", CreateGoodsReceiptAsync).WithName("CreateGoodsReceipt");
        group.MapPost("/supplier-invoices", CreateSupplierInvoiceAsync).WithName("CreateSupplierInvoice");

        return endpoints;
    }

    private static async Task<IResult> CreatePurchaseOrderAsync(
        CreatePurchaseOrderRequest request,
        HttpContext httpContext,
        IPurchasingService service,
        CancellationToken cancellationToken)
    {
        var validation = request.Validate();
        if (!validation.IsValid)
        {
            return validation.ToProblem(httpContext);
        }

        try
        {
            var response = await service.CreatePurchaseOrderAsync(request, cancellationToken);
            return Results.Created($"/api/v1/purchasing/orders/{response.Id}", response);
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { code = "purchasing.invalid_order", message = exception.Message });
        }
    }

    private static async Task<IResult> ListPurchaseOrdersAsync(
        Guid organisationId,
        Guid? supplierId,
        int? pageNumber,
        int? pageSize,
        IPurchasingService service,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "purchasing.organisation_required" });
        }

        var response = await service.ListPurchaseOrdersAsync(
            new ListPurchasingDocumentsRequest(organisationId, supplierId, new PagedRequest(pageNumber ?? 1, pageSize ?? 50)),
            cancellationToken);

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateGoodsReceiptAsync(
        CreateGoodsReceiptRequest request,
        HttpContext httpContext,
        IPurchasingService service,
        CancellationToken cancellationToken)
    {
        var validation = request.Validate();
        if (!validation.IsValid)
        {
            return validation.ToProblem(httpContext);
        }

        try
        {
            var response = await service.CreateGoodsReceiptAsync(request, cancellationToken);
            return Results.Created($"/api/v1/purchasing/goods-receipts/{response.Id}", response);
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { code = "purchasing.invalid_receipt", message = exception.Message });
        }
    }

    private static async Task<IResult> CreateSupplierInvoiceAsync(
        CreateSupplierInvoiceRequest request,
        HttpContext httpContext,
        IPurchasingService service,
        CancellationToken cancellationToken)
    {
        var validation = request.Validate();
        if (!validation.IsValid)
        {
            return validation.ToProblem(httpContext);
        }

        try
        {
            var response = await service.CreateSupplierInvoiceAsync(request, cancellationToken);
            return Results.Created($"/api/v1/purchasing/supplier-invoices/{response.Id}", response);
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { code = "purchasing.invalid_invoice", message = exception.Message });
        }
    }
}
