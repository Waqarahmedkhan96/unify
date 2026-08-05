using Microsoft.EntityFrameworkCore;
using Unify.Erp.Application.Accounting;
using Unify.Erp.Contracts.Accounting;
using Unify.Erp.Domain.Accounting;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Accounting;

public sealed class AccountingService : IAccountingService
{
    private readonly ApplicationDbContext _dbContext;

    public AccountingService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AccountType>(request.Type, ignoreCase: true, out var accountType))
        {
            throw new InvalidOperationException("Account type is invalid.");
        }

        await EnsureOrganisationAsync(request.OrganisationId, cancellationToken);
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        var duplicate = await _dbContext.Accounts.AnyAsync(
            account => account.OrganisationId == request.OrganisationId && account.Code == normalizedCode,
            cancellationToken);
        if (duplicate)
        {
            throw new InvalidOperationException("Account code already exists for the organisation.");
        }

        var account = new Account(Guid.NewGuid(), request.OrganisationId, request.Code, request.Name, accountType);
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(account);
    }

    public async Task<IReadOnlyCollection<AccountResponse>> ListAccountsAsync(Guid organisationId, CancellationToken cancellationToken)
    {
        return await _dbContext.Accounts.AsNoTracking()
            .Where(account => account.OrganisationId == organisationId)
            .OrderBy(account => account.Code)
            .Select(account => ToResponse(account))
            .ToListAsync(cancellationToken);
    }

    public async Task<FiscalPeriodResponse> CreateFiscalPeriodAsync(
        CreateFiscalPeriodRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureOrganisationAsync(request.OrganisationId, cancellationToken);
        var overlaps = await _dbContext.FiscalPeriods.AnyAsync(
            period => period.OrganisationId == request.OrganisationId
                && request.StartsOn <= period.EndsOn
                && request.EndsOn >= period.StartsOn,
            cancellationToken);
        if (overlaps)
        {
            throw new InvalidOperationException("Fiscal period overlaps an existing period.");
        }

        var period = new FiscalPeriod(Guid.NewGuid(), request.OrganisationId, request.Name, request.StartsOn, request.EndsOn);
        _dbContext.FiscalPeriods.Add(period);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(period);
    }

    public async Task<IReadOnlyCollection<FiscalPeriodResponse>> ListFiscalPeriodsAsync(
        Guid organisationId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.FiscalPeriods.AsNoTracking()
            .Where(period => period.OrganisationId == organisationId)
            .OrderBy(period => period.StartsOn)
            .Select(period => ToResponse(period))
            .ToListAsync(cancellationToken);
    }

    public async Task<JournalEntryResponse> CreateJournalEntryAsync(
        CreateJournalEntryRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Lines.Count < 2)
        {
            throw new InvalidOperationException("Journal entry requires at least two lines.");
        }

        var totalDebit = request.Lines.Sum(line => line.Debit);
        var totalCredit = request.Lines.Sum(line => line.Credit);
        if (totalDebit != totalCredit)
        {
            throw new InvalidOperationException("Journal entry must balance.");
        }

        var period = await _dbContext.FiscalPeriods.SingleOrDefaultAsync(
            item => item.OrganisationId == request.OrganisationId && item.StartsOn <= request.JournalDate && item.EndsOn >= request.JournalDate,
            cancellationToken);
        if (period is null || !period.IsOpen)
        {
            throw new InvalidOperationException("Journal date is not in an open fiscal period.");
        }

        var accountIds = request.Lines.Select(line => line.AccountId).Distinct().ToArray();
        var activeAccountCount = await _dbContext.Accounts.CountAsync(
            account => account.OrganisationId == request.OrganisationId
                && accountIds.Contains(account.Id)
                && account.Status == AccountStatus.Active,
            cancellationToken);
        if (activeAccountCount != accountIds.Length)
        {
            throw new InvalidOperationException("One or more accounts do not exist for the organisation.");
        }

        var journalId = Guid.NewGuid();
        var journal = new JournalEntry(
            journalId,
            request.OrganisationId,
            period.Id,
            request.JournalNumber,
            request.JournalDate,
            request.Description);
        var lines = request.Lines.Select(line => new JournalLine(
            Guid.NewGuid(),
            request.OrganisationId,
            journalId,
            line.AccountId,
            line.Description,
            line.Debit,
            line.Credit)).ToList();

        _dbContext.JournalEntries.Add(journal);
        _dbContext.JournalLines.AddRange(lines);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(journal, lines);
    }

    private async Task EnsureOrganisationAsync(Guid organisationId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Organisations.AnyAsync(organisation => organisation.Id == organisationId, cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException("Organisation does not exist.");
        }
    }

    private static AccountResponse ToResponse(Account account)
    {
        return new AccountResponse(account.Id, account.OrganisationId, account.Code, account.Name, account.Type.ToString(), account.Status.ToString());
    }

    private static FiscalPeriodResponse ToResponse(FiscalPeriod period)
    {
        return new FiscalPeriodResponse(period.Id, period.OrganisationId, period.Name, period.StartsOn, period.EndsOn, period.Status.ToString());
    }

    private static JournalEntryResponse ToResponse(JournalEntry journal, IReadOnlyCollection<JournalLine> lines)
    {
        return new JournalEntryResponse(
            journal.Id,
            journal.OrganisationId,
            journal.FiscalPeriodId,
            journal.JournalNumber,
            journal.JournalDate,
            journal.Description,
            journal.Status.ToString(),
            lines.Select(line => new JournalLineResponse(line.Id, line.AccountId, line.Description, line.Debit, line.Credit)).ToList());
    }
}
