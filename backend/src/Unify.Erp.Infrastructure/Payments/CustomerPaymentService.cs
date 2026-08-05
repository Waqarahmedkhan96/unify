using Microsoft.EntityFrameworkCore;
using Unify.Erp.Application.Payments;
using Unify.Erp.Contracts.Payments;
using Unify.Erp.Domain.Customers;
using Unify.Erp.Domain.Finance;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Payments;

public sealed class CustomerPaymentService : ICustomerPaymentService
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerPaymentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomerPaymentResponse> CreateAsync(
        CreateCustomerPaymentRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureCustomerAsync(request.OrganisationId, request.BranchId, request.CustomerId, cancellationToken);
        await EnsureReceiptNumberIsUniqueAsync(request.OrganisationId, request.ReceiptNumber, cancellationToken);
        if (!Enum.TryParse<PaymentMethod>(request.Method, ignoreCase: true, out var method))
        {
            throw new InvalidOperationException("Payment method is invalid.");
        }

        var allocatedAmount = request.Allocations.Sum(allocation => allocation.Amount);
        if (allocatedAmount > request.Amount)
        {
            throw new InvalidOperationException("Allocations cannot exceed payment amount.");
        }

        var saleIds = request.Allocations.Select(allocation => allocation.SaleId).Distinct().ToArray();
        var validSaleCount = await _dbContext.Sales.CountAsync(
            sale => sale.OrganisationId == request.OrganisationId
                && sale.CustomerId == request.CustomerId
                && saleIds.Contains(sale.Id),
            cancellationToken);

        if (validSaleCount != saleIds.Length)
        {
            throw new InvalidOperationException("One or more allocations reference invalid sales.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var payment = new CustomerPayment(
            Guid.NewGuid(),
            request.OrganisationId,
            request.BranchId,
            request.CustomerId,
            request.ReceiptNumber,
            request.Amount,
            method,
            request.PaymentDateUtc,
            request.Notes);
        var allocations = request.Allocations.Select(allocation => new PaymentAllocation(
            Guid.NewGuid(),
            request.OrganisationId,
            payment.Id,
            allocation.SaleId,
            allocation.Amount)).ToList();

        _dbContext.CustomerPayments.Add(payment);
        _dbContext.PaymentAllocations.AddRange(allocations);
        _dbContext.CustomerLedgerEntries.Add(new CustomerLedgerEntry(
            Guid.NewGuid(),
            request.OrganisationId,
            request.CustomerId,
            CustomerLedgerEntryType.Payment,
            "CustomerPayment",
            payment.Id,
            0,
            payment.Amount,
            payment.PaymentDateUtc));

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ToResponse(payment);
    }

    public async Task<CustomerBalanceResponse> GetBalanceAsync(
        Guid organisationId,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var entries = await _dbContext.CustomerLedgerEntries
            .AsNoTracking()
            .Where(entry => entry.OrganisationId == organisationId && entry.CustomerId == customerId)
            .Select(entry => new { entry.Debit, entry.Credit })
            .ToListAsync(cancellationToken);
        var balance = entries.Sum(entry => entry.Debit - entry.Credit);

        return new CustomerBalanceResponse(organisationId, customerId, balance);
    }

    public async Task<IReadOnlyCollection<CustomerLedgerEntryResponse>> ListLedgerAsync(
        Guid organisationId,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.CustomerLedgerEntries
            .AsNoTracking()
            .Where(entry => entry.OrganisationId == organisationId && entry.CustomerId == customerId)
            .OrderBy(entry => entry.EntryDateUtc)
            .Select(entry => ToResponse(entry))
            .ToListAsync(cancellationToken);
    }

    private async Task EnsureCustomerAsync(
        Guid organisationId,
        Guid branchId,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers.SingleOrDefaultAsync(
            item => item.OrganisationId == organisationId && item.BranchId == branchId && item.Id == customerId,
            cancellationToken);

        if (customer is null)
        {
            throw new InvalidOperationException("Customer does not exist for the branch.");
        }

        if (customer.Status != CustomerStatus.Active)
        {
            throw new InvalidOperationException("Inactive customers cannot make payments.");
        }
    }

    private async Task EnsureReceiptNumberIsUniqueAsync(
        Guid organisationId,
        string receiptNumber,
        CancellationToken cancellationToken)
    {
        var normalizedReceiptNumber = receiptNumber.Trim().ToUpperInvariant();
        var exists = await _dbContext.CustomerPayments.AnyAsync(
            payment => payment.OrganisationId == organisationId && payment.ReceiptNumber == normalizedReceiptNumber,
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Receipt number already exists for the organisation.");
        }
    }

    private static CustomerPaymentResponse ToResponse(CustomerPayment payment)
    {
        return new CustomerPaymentResponse(
            payment.Id,
            payment.OrganisationId,
            payment.BranchId,
            payment.CustomerId,
            payment.ReceiptNumber,
            payment.Amount,
            payment.Method.ToString(),
            payment.PaymentDateUtc,
            payment.Notes);
    }

    private static CustomerLedgerEntryResponse ToResponse(CustomerLedgerEntry entry)
    {
        return new CustomerLedgerEntryResponse(
            entry.Id,
            entry.OrganisationId,
            entry.CustomerId,
            entry.EntryType.ToString(),
            entry.ReferenceType,
            entry.ReferenceId,
            entry.Debit,
            entry.Credit,
            entry.BalanceImpact,
            entry.EntryDateUtc);
    }
}
