using Microsoft.EntityFrameworkCore;
using Unify.Erp.Application.Sales;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Sales;
using Unify.Erp.Domain.Customers;
using Unify.Erp.Domain.Finance;
using Unify.Erp.Domain.Inventory;
using Unify.Erp.Domain.Products;
using Unify.Erp.Domain.Sales;
using Unify.Erp.Domain.Warehouses;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Sales;

public sealed class SalesService : ISalesService
{
    private readonly ApplicationDbContext _dbContext;

    public SalesService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SaleResponse> CreateAsync(CreateSaleRequest request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("Sale must contain at least one item.");
        }

        var warehouse = await GetWarehouseAsync(request.OrganisationId, request.WarehouseId, request.BranchId, cancellationToken);
        await EnsureCustomerAsync(request.OrganisationId, request.BranchId, request.CustomerId, cancellationToken);
        await EnsureInvoiceNumberIsUniqueAsync(request.OrganisationId, request.InvoiceNumber, cancellationToken);

        var productIds = request.Items.Select(item => item.ProductId).Distinct().ToArray();
        var products = await _dbContext.Products
            .Where(product => product.OrganisationId == request.OrganisationId && productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);

        if (products.Count != productIds.Length)
        {
            throw new InvalidOperationException("One or more products do not exist for the organisation.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var saleId = Guid.NewGuid();
        var saleItems = new List<SaleItem>();
        decimal subtotal = 0;
        decimal discountTotal = 0;
        decimal taxTotal = 0;

        foreach (var item in request.Items)
        {
            var product = products[item.ProductId];
            if (product.Status != ProductStatus.Active)
            {
                throw new InvalidOperationException("Inactive products cannot be sold.");
            }

            var lineSubtotal = item.Quantity * item.UnitPrice;
            var lineTotal = lineSubtotal - item.DiscountAmount + item.TaxAmount;
            if (lineTotal < 0)
            {
                throw new InvalidOperationException("Sale item total cannot be negative.");
            }

            subtotal += lineSubtotal;
            discountTotal += item.DiscountAmount;
            taxTotal += item.TaxAmount;
            saleItems.Add(new SaleItem(
                Guid.NewGuid(),
                request.OrganisationId,
                saleId,
                item.ProductId,
                item.Description,
                item.Quantity,
                item.UnitPrice,
                item.DiscountAmount,
                item.TaxAmount,
                lineTotal));

            if (product.IsInventoryTracked)
            {
                await ApplyStockIssueAsync(
                    request.OrganisationId,
                    request.BranchId,
                    warehouse.Id,
                    product.Id,
                    saleId,
                    item.Quantity,
                    cancellationToken);
            }
        }

        var sale = new Sale(
            saleId,
            request.OrganisationId,
            request.BranchId,
            request.WarehouseId,
            request.CustomerId,
            request.InvoiceNumber,
            request.SaleDateUtc,
            subtotal,
            discountTotal,
            taxTotal,
            subtotal - discountTotal + taxTotal);

        _dbContext.Sales.Add(sale);
        _dbContext.SaleItems.AddRange(saleItems);
        _dbContext.CustomerLedgerEntries.Add(new CustomerLedgerEntry(
            Guid.NewGuid(),
            request.OrganisationId,
            request.CustomerId,
            CustomerLedgerEntryType.SaleInvoice,
            "Sale",
            sale.Id,
            sale.GrandTotal,
            0,
            sale.SaleDateUtc));
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ToResponse(sale, saleItems);
    }

    public async Task<SaleResponse?> GetAsync(Guid organisationId, Guid saleId, CancellationToken cancellationToken)
    {
        var sale = await _dbContext.Sales
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.OrganisationId == organisationId && item.Id == saleId, cancellationToken);

        if (sale is null)
        {
            return null;
        }

        var items = await _dbContext.SaleItems
            .AsNoTracking()
            .Where(item => item.OrganisationId == organisationId && item.SaleId == saleId)
            .OrderBy(item => item.Description)
            .ToListAsync(cancellationToken);

        return ToResponse(sale, items);
    }

    public async Task<PagedResponse<SaleResponse>> ListAsync(ListSalesRequest request, CancellationToken cancellationToken)
    {
        var pageNumber = request.Page.NormalizedPageNumber;
        var pageSize = request.Page.NormalizedPageSize;
        var query = _dbContext.Sales
            .AsNoTracking()
            .Where(sale => sale.OrganisationId == request.OrganisationId);

        if (request.CustomerId.HasValue)
        {
            query = query.Where(sale => sale.CustomerId == request.CustomerId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var sales = await query
            .OrderByDescending(sale => sale.SaleDateUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var saleIds = sales.Select(sale => sale.Id).ToArray();
        var items = await _dbContext.SaleItems
            .AsNoTracking()
            .Where(item => item.OrganisationId == request.OrganisationId && saleIds.Contains(item.SaleId))
            .ToListAsync(cancellationToken);
        var responses = sales.Select(sale => ToResponse(
            sale,
            items.Where(item => item.SaleId == sale.Id).OrderBy(item => item.Description).ToList())).ToList();

        return new PagedResponse<SaleResponse>(responses, pageNumber, pageSize, totalCount);
    }

    private async Task ApplyStockIssueAsync(
        Guid organisationId,
        Guid branchId,
        Guid warehouseId,
        Guid productId,
        Guid saleId,
        decimal quantity,
        CancellationToken cancellationToken)
    {
        var balance = await _dbContext.StockBalances.SingleOrDefaultAsync(
            item => item.OrganisationId == organisationId && item.WarehouseId == warehouseId && item.ProductId == productId,
            cancellationToken);

        if (balance is null)
        {
            balance = new StockBalance(Guid.NewGuid(), organisationId, warehouseId, productId);
            _dbContext.StockBalances.Add(balance);
        }

        var movement = new StockMovement(
            Guid.NewGuid(),
            organisationId,
            branchId,
            warehouseId,
            productId,
            StockMovementType.SaleIssue,
            quantity,
            "Sale",
            saleId,
            "Sale stock issue",
            DateTimeOffset.UtcNow);

        balance.Apply(movement.SignedQuantity);
        _dbContext.StockMovements.Add(movement);
    }

    private async Task<Warehouse> GetWarehouseAsync(
        Guid organisationId,
        Guid warehouseId,
        Guid branchId,
        CancellationToken cancellationToken)
    {
        var warehouse = await _dbContext.Warehouses.SingleOrDefaultAsync(
            item => item.OrganisationId == organisationId && item.Id == warehouseId && item.BranchId == branchId,
            cancellationToken);

        return warehouse ?? throw new InvalidOperationException("Warehouse does not exist for the branch.");
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
            throw new InvalidOperationException("Inactive customers cannot be used for sales.");
        }
    }

    private async Task EnsureInvoiceNumberIsUniqueAsync(Guid organisationId, string invoiceNumber, CancellationToken cancellationToken)
    {
        var normalizedInvoiceNumber = invoiceNumber.Trim().ToUpperInvariant();
        var exists = await _dbContext.Sales.AnyAsync(
            sale => sale.OrganisationId == organisationId && sale.InvoiceNumber == normalizedInvoiceNumber,
            cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Invoice number already exists for the organisation.");
        }
    }

    private static SaleResponse ToResponse(Sale sale, IReadOnlyCollection<SaleItem> items)
    {
        return new SaleResponse(
            sale.Id,
            sale.OrganisationId,
            sale.BranchId,
            sale.WarehouseId,
            sale.CustomerId,
            sale.InvoiceNumber,
            sale.SaleDateUtc,
            sale.Subtotal,
            sale.DiscountTotal,
            sale.TaxTotal,
            sale.GrandTotal,
            sale.Status.ToString(),
            items.Select(ToResponse).ToList());
    }

    private static SaleItemResponse ToResponse(SaleItem item)
    {
        return new SaleItemResponse(
            item.Id,
            item.ProductId,
            item.Description,
            item.Quantity,
            item.UnitPrice,
            item.DiscountAmount,
            item.TaxAmount,
            item.LineTotal);
    }
}
