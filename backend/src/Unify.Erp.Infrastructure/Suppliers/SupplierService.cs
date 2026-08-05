using Microsoft.EntityFrameworkCore;
using Unify.Erp.Application.Suppliers;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Suppliers;
using Unify.Erp.Domain.Suppliers;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Suppliers;

public sealed class SupplierService : ISupplierService
{
    private readonly ApplicationDbContext _dbContext;

    public SupplierService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SupplierResponse> CreateAsync(
        CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var organisationExists = await _dbContext.Organisations
            .AnyAsync(organisation => organisation.Id == request.OrganisationId, cancellationToken);

        if (!organisationExists)
        {
            throw new InvalidOperationException("Organisation does not exist.");
        }

        var normalizedSupplierNumber = request.SupplierNumber.Trim().ToUpperInvariant();
        var duplicateExists = await _dbContext.Suppliers
            .AnyAsync(
                supplier => supplier.OrganisationId == request.OrganisationId
                    && supplier.SupplierNumber == normalizedSupplierNumber,
                cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException("Supplier number already exists for the organisation.");
        }

        var supplier = new Supplier(
            Guid.NewGuid(),
            request.OrganisationId,
            request.SupplierNumber,
            request.DisplayName,
            request.LegalName,
            request.Phone,
            request.Email,
            request.TaxNumber);

        _dbContext.Suppliers.Add(supplier);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(supplier);
    }

    public async Task<SupplierResponse?> GetAsync(
        Guid organisationId,
        Guid supplierId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Suppliers
            .AsNoTracking()
            .Where(supplier => supplier.OrganisationId == organisationId && supplier.Id == supplierId)
            .Select(supplier => ToResponse(supplier))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResponse<SupplierResponse>> ListAsync(
        ListSuppliersRequest request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.Page.NormalizedPageNumber;
        var pageSize = request.Page.NormalizedPageSize;
        var query = _dbContext.Suppliers
            .AsNoTracking()
            .Where(supplier => supplier.OrganisationId == request.OrganisationId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpperInvariant();
            query = query.Where(supplier =>
                supplier.SupplierNumber.Contains(search)
                || supplier.DisplayName.ToUpper().Contains(search)
                || (supplier.Phone != null && supplier.Phone.Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(supplier => supplier.DisplayName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(supplier => ToResponse(supplier))
            .ToListAsync(cancellationToken);

        return new PagedResponse<SupplierResponse>(items, pageNumber, pageSize, totalCount);
    }

    public async Task<bool> DeactivateAsync(
        Guid organisationId,
        Guid supplierId,
        CancellationToken cancellationToken)
    {
        var supplier = await _dbContext.Suppliers
            .SingleOrDefaultAsync(
                supplier => supplier.OrganisationId == organisationId && supplier.Id == supplierId,
                cancellationToken);

        if (supplier is null)
        {
            return false;
        }

        supplier.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static SupplierResponse ToResponse(Supplier supplier)
    {
        return new SupplierResponse(
            supplier.Id,
            supplier.OrganisationId,
            supplier.SupplierNumber,
            supplier.DisplayName,
            supplier.LegalName,
            supplier.Phone,
            supplier.Email,
            supplier.TaxNumber,
            supplier.Status.ToString(),
            supplier.CreatedAtUtc);
    }
}
