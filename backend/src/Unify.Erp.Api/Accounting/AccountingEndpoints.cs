using Unify.Erp.Application.Accounting;
using Unify.Erp.Contracts.Accounting;

namespace Unify.Erp.Api.Accounting;

public static class AccountingEndpoints
{
    public static IEndpointRouteBuilder MapAccountingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/accounting")
            .RequireAuthorization()
            .WithTags("Accounting");

        group.MapPost("/accounts", CreateAccountAsync).WithName("CreateAccount");
        group.MapGet("/accounts", ListAccountsAsync).WithName("ListAccounts");
        group.MapPost("/fiscal-periods", CreateFiscalPeriodAsync).WithName("CreateFiscalPeriod");
        group.MapGet("/fiscal-periods", ListFiscalPeriodsAsync).WithName("ListFiscalPeriods");
        group.MapPost("/journals", CreateJournalAsync).WithName("CreateJournal");
        return endpoints;
    }

    private static async Task<IResult> CreateAccountAsync(CreateAccountRequest request, IAccountingService service, CancellationToken cancellationToken)
    {
        try { return Results.Created("/api/v1/accounting/accounts", await service.CreateAccountAsync(request, cancellationToken)); }
        catch (InvalidOperationException exception) { return Results.BadRequest(new { code = "accounting.invalid_account", message = exception.Message }); }
    }

    private static async Task<IResult> ListAccountsAsync(Guid organisationId, IAccountingService service, CancellationToken cancellationToken)
    {
        return organisationId == Guid.Empty
            ? Results.BadRequest(new { code = "accounting.organisation_required" })
            : Results.Ok(await service.ListAccountsAsync(organisationId, cancellationToken));
    }

    private static async Task<IResult> CreateFiscalPeriodAsync(CreateFiscalPeriodRequest request, IAccountingService service, CancellationToken cancellationToken)
    {
        try { return Results.Created("/api/v1/accounting/fiscal-periods", await service.CreateFiscalPeriodAsync(request, cancellationToken)); }
        catch (InvalidOperationException exception) { return Results.BadRequest(new { code = "accounting.invalid_period", message = exception.Message }); }
    }

    private static async Task<IResult> ListFiscalPeriodsAsync(Guid organisationId, IAccountingService service, CancellationToken cancellationToken)
    {
        return organisationId == Guid.Empty
            ? Results.BadRequest(new { code = "accounting.organisation_required" })
            : Results.Ok(await service.ListFiscalPeriodsAsync(organisationId, cancellationToken));
    }

    private static async Task<IResult> CreateJournalAsync(CreateJournalEntryRequest request, IAccountingService service, CancellationToken cancellationToken)
    {
        try { return Results.Created("/api/v1/accounting/journals", await service.CreateJournalEntryAsync(request, cancellationToken)); }
        catch (InvalidOperationException exception) { return Results.BadRequest(new { code = "accounting.invalid_journal", message = exception.Message }); }
    }
}
