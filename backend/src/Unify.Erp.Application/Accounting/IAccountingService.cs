using Unify.Erp.Contracts.Accounting;

namespace Unify.Erp.Application.Accounting;

public interface IAccountingService
{
    Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AccountResponse>> ListAccountsAsync(Guid organisationId, CancellationToken cancellationToken);
    Task<FiscalPeriodResponse> CreateFiscalPeriodAsync(CreateFiscalPeriodRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<FiscalPeriodResponse>> ListFiscalPeriodsAsync(Guid organisationId, CancellationToken cancellationToken);
    Task<JournalEntryResponse> CreateJournalEntryAsync(CreateJournalEntryRequest request, CancellationToken cancellationToken);
}
