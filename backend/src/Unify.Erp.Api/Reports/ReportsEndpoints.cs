using Microsoft.EntityFrameworkCore;
using Unify.Erp.Contracts.Auth;
using Unify.Erp.Contracts.Reports;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Api.Reports;

public static class ReportsEndpoints
{
    public static IEndpointRouteBuilder MapReportsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/reports")
            .RequireAuthorization()
            .RequireAuthorization(PermissionNames.SalesManage)
            .WithTags("Reports");

        group.MapGet("/sales", GetSalesReportAsync).WithName("GetSalesReport");

        return endpoints;
    }

    private static async Task<IResult> GetSalesReportAsync(
        Guid organisationId,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        Guid? customerId,
        Guid? productId,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (organisationId == Guid.Empty)
        {
            return Results.BadRequest(new { code = "reports.organisation_required" });
        }

        var salesQuery = dbContext.Sales
            .AsNoTracking()
            .Where(sale => sale.OrganisationId == organisationId);

        if (fromUtc.HasValue)
        {
            salesQuery = salesQuery.Where(sale => sale.SaleDateUtc >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            salesQuery = salesQuery.Where(sale => sale.SaleDateUtc <= toUtc.Value);
        }

        if (customerId.HasValue)
        {
            salesQuery = salesQuery.Where(sale => sale.CustomerId == customerId.Value);
        }

        if (productId.HasValue)
        {
            var saleIdsForProduct = dbContext.SaleItems
                .AsNoTracking()
                .Where(item => item.OrganisationId == organisationId && item.ProductId == productId.Value)
                .Select(item => item.SaleId);
            salesQuery = salesQuery.Where(sale => saleIdsForProduct.Contains(sale.Id));
        }

        var sales = await salesQuery
            .OrderByDescending(sale => sale.SaleDateUtc)
            .Take(500)
            .ToListAsync(cancellationToken);
        var saleIds = sales.Select(sale => sale.Id).ToArray();
        var saleItems = await dbContext.SaleItems
            .AsNoTracking()
            .Where(item => item.OrganisationId == organisationId && saleIds.Contains(item.SaleId))
            .Where(item => !productId.HasValue || item.ProductId == productId.Value)
            .ToListAsync(cancellationToken);
        var productIds = saleItems.Select(item => item.ProductId).Distinct().ToArray();
        var products = await dbContext.Products
            .AsNoTracking()
            .Where(product => product.OrganisationId == organisationId && productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);
        var customerIds = sales.Select(sale => sale.CustomerId).Distinct().ToArray();
        var customers = await dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.OrganisationId == organisationId && customerIds.Contains(customer.Id))
            .ToDictionaryAsync(customer => customer.Id, cancellationToken);

        var productRows = saleItems
            .GroupBy(item => item.ProductId)
            .Select(group =>
            {
                products.TryGetValue(group.Key, out var product);
                return new SalesReportProductRow(
                    group.Key,
                    product?.ProductCode ?? string.Empty,
                    product?.Name ?? "Unknown product",
                    group.Sum(item => item.Quantity),
                    group.Sum(item => item.LineTotal));
            })
            .OrderByDescending(row => row.SalesTotal)
            .ToArray();

        var invoiceRows = sales
            .Select(sale =>
            {
                customers.TryGetValue(sale.CustomerId, out var customer);
                return new SalesReportInvoiceRow(
                    sale.Id,
                    sale.InvoiceNumber,
                    sale.SaleDateUtc,
                    sale.CustomerId,
                    customer?.DisplayName ?? "Unknown customer",
                    sale.GrandTotal);
            })
            .ToArray();

        var response = new SalesReportResponse(
            organisationId,
            fromUtc,
            toUtc,
            customerId,
            productId,
            sales.Count,
            saleItems.Sum(item => item.Quantity),
            sales.Sum(sale => sale.Subtotal),
            sales.Sum(sale => sale.DiscountTotal),
            sales.Sum(sale => sale.TaxTotal),
            sales.Sum(sale => sale.GrandTotal),
            productRows,
            invoiceRows);

        return Results.Ok(response);
    }
}
