using Microsoft.AspNetCore.SignalR;
using Unify.Erp.Api.Common;
using Unify.Erp.Api.Realtime;
using Unify.Erp.Application.Sales;
using Unify.Erp.Contracts.Auth;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Sales;

namespace Unify.Erp.Api.Sales;

public static class SalesEndpoints
{
    public static IEndpointRouteBuilder MapSalesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/sales")
            .RequireAuthorization()
            .RequireAuthorization(PermissionNames.SalesManage)
            .WithTags("Sales");

        group.MapPost("/", CreateSaleAsync).WithName("CreateSale");
        group.MapGet("/", ListSalesAsync).WithName("ListSales");
        group.MapGet("/{saleId:guid}", GetSaleAsync).WithName("GetSale");

        return endpoints;
    }

    private static async Task<IResult> CreateSaleAsync(
        CreateSaleRequest request,
        HttpContext httpContext,
        ISalesService service,
        IHubContext<OperationsHub> hubContext,
        CancellationToken cancellationToken)
    {
        var validationResult = request.Validate();
        if (!validationResult.IsValid)
        {
            return validationResult.ToProblem(httpContext);
        }

        try
        {
            var response = await service.CreateAsync(request, cancellationToken);
            await hubContext.Clients
                .Group(OperationsHub.OrganisationGroup(request.OrganisationId.ToString()))
                .SendAsync("operationChanged", new OperationChangedEvent("sales", "created", request.OrganisationId, response.Id, DateTimeOffset.UtcNow), cancellationToken);
            return Results.Created($"/api/v1/sales/{response.Id}", response);
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { code = "sales.invalid_sale", field = exception.ParamName });
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { code = "sales.invalid_sale", message = exception.Message });
        }
    }

    private static async Task<IResult> ListSalesAsync(
        Guid organisationId,
        Guid? customerId,
        int? pageNumber,
        int? pageSize,
        ISalesService service,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "sales.organisation_required" });
        }

        var response = await service.ListAsync(
            new ListSalesRequest(organisationId, customerId, new PagedRequest(pageNumber ?? 1, pageSize ?? 50)),
            cancellationToken);

        return Results.Ok(response);
    }

    private static async Task<IResult> GetSaleAsync(
        Guid saleId,
        Guid organisationId,
        ISalesService service,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "sales.organisation_required" });
        }

        var response = await service.GetAsync(organisationId, saleId, cancellationToken);

        return response is null ? Results.NotFound() : Results.Ok(response);
    }
}
