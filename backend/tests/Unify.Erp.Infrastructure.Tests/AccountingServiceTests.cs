using Microsoft.EntityFrameworkCore;
using Unify.Erp.Contracts.Accounting;
using Unify.Erp.Contracts.Platform;
using Unify.Erp.Infrastructure.Accounting;
using Unify.Erp.Infrastructure.Persistence;
using Unify.Erp.Infrastructure.Platform;

namespace Unify.Erp.Infrastructure.Tests;

public sealed class AccountingServiceTests
{
    [Fact]
    public async Task Creates_balanced_journal_in_open_period()
    {
        using var dbContext = CreateDbContext();
        var (organisationId, cashId, revenueId) = await CreateFixtureAsync(dbContext);
        var service = new AccountingService(dbContext);

        var journal = await service.CreateJournalEntryAsync(
            new CreateJournalEntryRequest(
                organisationId,
                "jrn-001",
                new DateOnly(2026, 8, 5),
                "Opening sale",
                [
                    new CreateJournalLineRequest(cashId, "Cash", 100, 0),
                    new CreateJournalLineRequest(revenueId, "Revenue", 0, 100)
                ]),
            CancellationToken.None);

        Assert.Equal("JRN-001", journal.JournalNumber);
        Assert.Equal(2, journal.Lines.Count);
    }

    [Fact]
    public async Task Rejects_unbalanced_journal()
    {
        using var dbContext = CreateDbContext();
        var (organisationId, cashId, revenueId) = await CreateFixtureAsync(dbContext);
        var service = new AccountingService(dbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateJournalEntryAsync(
                new CreateJournalEntryRequest(
                    organisationId,
                    "jrn-001",
                    new DateOnly(2026, 8, 5),
                    "Bad journal",
                    [
                        new CreateJournalLineRequest(cashId, "Cash", 100, 0),
                        new CreateJournalLineRequest(revenueId, "Revenue", 0, 90)
                    ]),
                CancellationToken.None));

        Assert.Equal("Journal entry must balance.", exception.Message);
    }

    [Fact]
    public async Task Rejects_overlapping_fiscal_period()
    {
        using var dbContext = CreateDbContext();
        var organisation = await new OrganisationService(dbContext).CreateAsync(
            new CreateOrganisationRequest("Royal LPG Private Limited", "Royal LPG", "PKR", "Asia/Karachi"),
            CancellationToken.None);
        var service = new AccountingService(dbContext);
        await service.CreateFiscalPeriodAsync(
            new CreateFiscalPeriodRequest(organisation.Id, "August", new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateFiscalPeriodAsync(
                new CreateFiscalPeriodRequest(organisation.Id, "Overlap", new DateOnly(2026, 8, 15), new DateOnly(2026, 9, 15)),
                CancellationToken.None));

        Assert.Equal("Fiscal period overlaps an existing period.", exception.Message);
    }

    private static async Task<(Guid OrganisationId, Guid CashId, Guid RevenueId)> CreateFixtureAsync(ApplicationDbContext dbContext)
    {
        var organisation = await new OrganisationService(dbContext).CreateAsync(
            new CreateOrganisationRequest("Royal LPG Private Limited", "Royal LPG", "PKR", "Asia/Karachi"),
            CancellationToken.None);
        var service = new AccountingService(dbContext);
        var cash = await service.CreateAccountAsync(
            new CreateAccountRequest(organisation.Id, "cash", "Cash", "Asset"),
            CancellationToken.None);
        var revenue = await service.CreateAccountAsync(
            new CreateAccountRequest(organisation.Id, "sales", "Sales Revenue", "Revenue"),
            CancellationToken.None);
        await service.CreateFiscalPeriodAsync(
            new CreateFiscalPeriodRequest(organisation.Id, "August", new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31)),
            CancellationToken.None);

        return (organisation.Id, cash.Id, revenue.Id);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
