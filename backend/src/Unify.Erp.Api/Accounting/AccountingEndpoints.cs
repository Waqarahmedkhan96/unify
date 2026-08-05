using Microsoft.AspNetCore.SignalR;
using Unify.Erp.Api.Realtime;
using Unify.Erp.Application.Accounting;
using Unify.Erp.Contracts.Auth;
using Unify.Erp.Contracts.Accounting;

namespace Unify.Erp.Api.Accounting;

public static class AccountingEndpoints
{
    public static IEndpointRouteBuilder MapAccountingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/accounting")
            .RequireAuthorization()
            .RequireAuthorization(PermissionNames.AccountingManage)
            .WithTags("Accounting");

        group.MapPost("/accounts", CreateAccountAsync).WithName("CreateAccount");
        group.MapGet("/accounts", ListAccountsAsync).WithName("ListAccounts");
        group.MapPost("/fiscal-periods", CreateFiscalPeriodAsync).WithName("CreateFiscalPeriod");
        group.MapGet("/fiscal-periods", ListFiscalPeriodsAsync).WithName("ListFiscalPeriods");
        group.MapPost("/journals", CreateJournalAsync).WithName("CreateJournal");
        return endpoints;
    }

    private static async Task<IResult> CreateAccountAsync(CreateAccountRequest request, IAccountingService service, IHubContext<OperationsHub> hubContext, CancellationToken cancellationToken)
    {
        try
        {
            var response = await service.CreateAccountAsync(request, cancellationToken);
            await hubContext.Clients
                .Group(OperationsHub.OrganisationGroup(request.OrganisationId.ToString()))
                .SendAsync("operationChanged", new OperationChangedEvent("accounting", "account-created", request.OrganisationId, response.Id, DateTimeOffset.UtcNow), cancellationToken);
            return Results.Created("/api/v1/accounting/accounts", response);
        }
        catch (InvalidOperationException exception) { return Results.BadRequest(new { code = "accounting.invalid_account", message = exception.Message }); }
    }

    private static async Task<IResult> ListAccountsAsync(Guid organisationId, IAccountingService service, CancellationToken cancellationToken)
    {
        return organisationId == Guid.Empty
            ? Results.BadRequest(new { code = "accounting.organisation_required" })
            : Results.Ok(await service.ListAccountsAsync(organisationId, cancellationToken));
    }

    private static async Task<IResult> CreateFiscalPeriodAsync(CreateFiscalPeriodRequest request, IAccountingService service, IHubContext<OperationsHub> hubContext, CancellationToken cancellationToken)
    {
        try
        {
            var response = await service.CreateFiscalPeriodAsync(request, cancellationToken);
            await hubContext.Clients
                .Group(OperationsHub.OrganisationGroup(request.OrganisationId.ToString()))
                .SendAsync("operationChanged", new OperationChangedEvent("accounting", "fiscal-period-created", request.OrganisationId, response.Id, DateTimeOffset.UtcNow), cancellationToken);
            return Results.Created("/api/v1/accounting/fiscal-periods", response);
        }
        catch (InvalidOperationException exception) { return Results.BadRequest(new { code = "accounting.invalid_period", message = exception.Message }); }
    }

    private static async Task<IResult> ListFiscalPeriodsAsync(Guid organisationId, IAccountingService service, CancellationToken cancellationToken)
    {
        return organisationId == Guid.Empty
            ? Results.BadRequest(new { code = "accounting.organisation_required" })
            : Results.Ok(await service.ListFiscalPeriodsAsync(organisationId, cancellationToken));
    }

    private static async Task<IResult> CreateJournalAsync(CreateJournalEntryRequest request, IAccountingService service, IHubContext<OperationsHub> hubContext, CancellationToken cancellationToken)
    {
        try
        {
            var response = await service.CreateJournalEntryAsync(request, cancellationToken);
            await hubContext.Clients
                .Group(OperationsHub.OrganisationGroup(request.OrganisationId.ToString()))
                .SendAsync("operationChanged", new OperationChangedEvent("accounting", "journal-created", request.OrganisationId, response.Id, DateTimeOffset.UtcNow), cancellationToken);
            return Results.Created("/api/v1/accounting/journals", response);
        }
        catch (InvalidOperationException exception) { return Results.BadRequest(new { code = "accounting.invalid_journal", message = exception.Message }); }
    }
}
