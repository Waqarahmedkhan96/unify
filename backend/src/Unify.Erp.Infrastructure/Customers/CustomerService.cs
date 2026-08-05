using Microsoft.EntityFrameworkCore;
using Unify.Erp.Application.Customers;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Customers;
using Unify.Erp.Domain.Customers;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Customers;

public sealed class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CustomerResponse> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var branchExists = await _dbContext.Branches
            .AnyAsync(
                branch => branch.Id == request.BranchId && branch.OrganisationId == request.OrganisationId,
                cancellationToken);

        if (!branchExists)
        {
            throw new InvalidOperationException("Branch does not exist for the organisation.");
        }

        var normalizedCustomerNumber = request.CustomerNumber.Trim().ToUpperInvariant();
        var duplicateExists = await _dbContext.Customers
            .AnyAsync(
                customer => customer.OrganisationId == request.OrganisationId
                    && customer.CustomerNumber == normalizedCustomerNumber,
                cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException("Customer number already exists for the organisation.");
        }

        var customer = new Customer(
            Guid.NewGuid(),
            request.OrganisationId,
            request.BranchId,
            request.CustomerNumber,
            request.DisplayName,
            request.LegalName,
            request.Phone,
            request.Email,
            request.TaxNumber,
            request.CreditLimit);

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(customer);
    }

    public async Task<CustomerResponse?> GetAsync(
        Guid organisationId,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.OrganisationId == organisationId && customer.Id == customerId)
            .Select(customer => ToResponse(customer))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResponse<CustomerResponse>> ListAsync(
        ListCustomersRequest request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.Page.NormalizedPageNumber;
        var pageSize = request.Page.NormalizedPageSize;
        var query = _dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.OrganisationId == request.OrganisationId);

        if (request.BranchId.HasValue)
        {
            query = query.Where(customer => customer.BranchId == request.BranchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpperInvariant();
            query = query.Where(customer =>
                customer.CustomerNumber.Contains(search)
                || customer.DisplayName.ToUpper().Contains(search)
                || (customer.Phone != null && customer.Phone.Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(customer => customer.DisplayName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(customer => ToResponse(customer))
            .ToListAsync(cancellationToken);

        return new PagedResponse<CustomerResponse>(items, pageNumber, pageSize, totalCount);
    }

    public async Task<bool> DeactivateAsync(
        Guid organisationId,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers
            .SingleOrDefaultAsync(
                customer => customer.OrganisationId == organisationId && customer.Id == customerId,
                cancellationToken);

        if (customer is null)
        {
            return false;
        }

        customer.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static CustomerResponse ToResponse(Customer customer)
    {
        return new CustomerResponse(
            customer.Id,
            customer.OrganisationId,
            customer.BranchId,
            customer.CustomerNumber,
            customer.DisplayName,
            customer.LegalName,
            customer.Phone,
            customer.Email,
            customer.TaxNumber,
            customer.CreditLimit,
            customer.Status.ToString(),
            customer.CreatedAtUtc);
    }
}
