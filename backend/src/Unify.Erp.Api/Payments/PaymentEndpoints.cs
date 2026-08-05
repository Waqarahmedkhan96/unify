using Unify.Erp.Api.Common;
using Unify.Erp.Application.Payments;
using Unify.Erp.Contracts.Payments;

namespace Unify.Erp.Api.Payments;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/payments")
            .RequireAuthorization()
            .WithTags("Payments");

        group.MapPost("/customers", CreateCustomerPaymentAsync).WithName("CreateCustomerPayment");
        group.MapGet("/customers/{customerId:guid}/balance", GetCustomerBalanceAsync).WithName("GetCustomerBalance");
        group.MapGet("/customers/{customerId:guid}/ledger", ListCustomerLedgerAsync).WithName("ListCustomerLedger");

        return endpoints;
    }

    private static async Task<IResult> CreateCustomerPaymentAsync(
        CreateCustomerPaymentRequest request,
        HttpContext httpContext,
        ICustomerPaymentService service,
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
            return Results.Created($"/api/v1/payments/customers/{response.Id}", response);
        }
        catch (ArgumentException exception)
        {
            return Results.BadRequest(new { code = "payments.invalid_payment", field = exception.ParamName });
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { code = "payments.invalid_payment", message = exception.Message });
        }
    }

    private static async Task<IResult> GetCustomerBalanceAsync(
        Guid customerId,
        Guid organisationId,
        ICustomerPaymentService service,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "payments.organisation_required" });
        }

        return Results.Ok(await service.GetBalanceAsync(organisationId, customerId, cancellationToken));
    }

    private static async Task<IResult> ListCustomerLedgerAsync(
        Guid customerId,
        Guid organisationId,
        ICustomerPaymentService service,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "payments.organisation_required" });
        }

        return Results.Ok(await service.ListLedgerAsync(organisationId, customerId, cancellationToken));
    }
}
