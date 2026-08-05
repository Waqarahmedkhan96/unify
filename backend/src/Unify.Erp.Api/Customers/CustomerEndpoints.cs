using Microsoft.AspNetCore.SignalR;
using Unify.Erp.Api.Common;
using Unify.Erp.Api.Realtime;
using Unify.Erp.Application.Customers;
using Unify.Erp.Contracts.Auth;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Customers;

namespace Unify.Erp.Api.Customers;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/customers")
            .RequireAuthorization()
            .RequireAuthorization(PermissionNames.CustomersManage)
            .WithTags("Customers");

        group.MapPost("/", CreateCustomerAsync)
            .WithName("CreateCustomer");

        group.MapGet("/", ListCustomersAsync)
            .WithName("ListCustomers");

        group.MapGet("/{customerId:guid}", GetCustomerAsync)
            .WithName("GetCustomer");

        group.MapPost("/{customerId:guid}/deactivate", DeactivateCustomerAsync)
            .WithName("DeactivateCustomer");

        return endpoints;
    }

    private static async Task<IResult> CreateCustomerAsync(
        CreateCustomerRequest request,
        HttpContext httpContext,
        ICustomerService customerService,
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
            var response = await customerService.CreateAsync(request, cancellationToken);
            await hubContext.Clients
                .Group(OperationsHub.OrganisationGroup(request.OrganisationId.ToString()))
                .SendAsync("operationChanged", new OperationChangedEvent("customers", "created", request.OrganisationId, response.Id, DateTimeOffset.UtcNow), cancellationToken);

            return Results.Created($"/api/v1/customers/{response.Id}", response);
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { code = "customers.invalid_customer", field = exception.ParamName });
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { code = "customers.invalid_customer", message = exception.Message });
        }
    }

    private static async Task<IResult> ListCustomersAsync(
        Guid organisationId,
        Guid? branchId,
        string? search,
        int? pageNumber,
        int? pageSize,
        ICustomerService customerService,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "customers.organisation_required" });
        }

        var response = await customerService.ListAsync(
            new ListCustomersRequest(
                organisationId,
                branchId,
                search,
                new PagedRequest(pageNumber ?? 1, pageSize ?? 50)),
            cancellationToken);

        return Results.Ok(response);
    }

    private static async Task<IResult> GetCustomerAsync(
        Guid customerId,
        Guid organisationId,
        ICustomerService customerService,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "customers.organisation_required" });
        }

        var response = await customerService.GetAsync(organisationId, customerId, cancellationToken);

        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<IResult> DeactivateCustomerAsync(
        Guid customerId,
        Guid organisationId,
        ICustomerService customerService,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "customers.organisation_required" });
        }

        var deactivated = await customerService.DeactivateAsync(organisationId, customerId, cancellationToken);

        return deactivated ? Results.NoContent() : Results.NotFound();
    }
}
