using Microsoft.EntityFrameworkCore;
using Unify.Erp.Application.Purchasing;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Purchasing;
using Unify.Erp.Domain.Inventory;
using Unify.Erp.Domain.Products;
using Unify.Erp.Domain.Purchasing;
using Unify.Erp.Domain.Suppliers;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Purchasing;

public sealed class PurchasingService : IPurchasingService
{
    private readonly ApplicationDbContext _dbContext;

    public PurchasingService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PurchaseOrderResponse> CreatePurchaseOrderAsync(
        CreatePurchaseOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("Purchase order must contain at least one item.");
        }

        await EnsureSupplierAsync(request.OrganisationId, request.SupplierId, cancellationToken);
        await EnsureBranchAsync(request.OrganisationId, request.BranchId, cancellationToken);
        await EnsureUniqueAsync(
            _dbContext.PurchaseOrders.AnyAsync(
                order => order.OrganisationId == request.OrganisationId
                    && order.OrderNumber == request.OrderNumber.Trim().ToUpperInvariant(),
                cancellationToken),
            "Purchase order number already exists for the organisation.");

        var orderId = Guid.NewGuid();
        var items = CreateOrderItems(request.OrganisationId, orderId, request.Items);
        var subtotal = items.Sum(item => item.Quantity * item.UnitCost);
        var taxTotal = items.Sum(item => item.TaxAmount);
        var order = new PurchaseOrder(
            orderId,
            request.OrganisationId,
            request.BranchId,
            request.SupplierId,
            request.OrderNumber,
            request.OrderDateUtc,
            subtotal,
            taxTotal,
            subtotal + taxTotal);

        await EnsureProductsAsync(request.OrganisationId, request.Items.Select(item => item.ProductId), cancellationToken);
        _dbContext.PurchaseOrders.Add(order);
        _dbContext.PurchaseOrderItems.AddRange(items);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(order, items);
    }

    public async Task<PagedResponse<PurchaseOrderResponse>> ListPurchaseOrdersAsync(
        ListPurchasingDocumentsRequest request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.Page.NormalizedPageNumber;
        var pageSize = request.Page.NormalizedPageSize;
        var query = _dbContext.PurchaseOrders
            .AsNoTracking()
            .Where(order => order.OrganisationId == request.OrganisationId);

        if (request.SupplierId.HasValue)
        {
            query = query.Where(order => order.SupplierId == request.SupplierId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(order => order.OrderDateUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var orderIds = orders.Select(order => order.Id).ToArray();
        var items = await _dbContext.PurchaseOrderItems
            .AsNoTracking()
            .Where(item => item.OrganisationId == request.OrganisationId && orderIds.Contains(item.PurchaseOrderId))
            .ToListAsync(cancellationToken);

        var responses = orders
            .Select(order => ToResponse(order, items.Where(item => item.PurchaseOrderId == order.Id).ToList()))
            .ToList();

        return new PagedResponse<PurchaseOrderResponse>(responses, pageNumber, pageSize, totalCount);
    }

    public async Task<GoodsReceiptResponse> CreateGoodsReceiptAsync(
        CreateGoodsReceiptRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("Goods receipt must contain at least one item.");
        }

        await EnsureSupplierAsync(request.OrganisationId, request.SupplierId, cancellationToken);
        var warehouse = await _dbContext.Warehouses.SingleOrDefaultAsync(
            item => item.OrganisationId == request.OrganisationId
                && item.Id == request.WarehouseId
                && item.BranchId == request.BranchId,
            cancellationToken);
        if (warehouse is null)
        {
            throw new InvalidOperationException("Warehouse does not exist for the branch.");
        }

        await EnsureProductsAsync(request.OrganisationId, request.Items.Select(item => item.ProductId), cancellationToken);
        await EnsureUniqueAsync(
            _dbContext.GoodsReceipts.AnyAsync(
                receipt => receipt.OrganisationId == request.OrganisationId
                    && receipt.ReceiptNumber == request.ReceiptNumber.Trim().ToUpperInvariant(),
                cancellationToken),
            "Goods receipt number already exists for the organisation.");

        PurchaseOrder? purchaseOrder = null;
        if (request.PurchaseOrderId.HasValue)
        {
            purchaseOrder = await _dbContext.PurchaseOrders.SingleOrDefaultAsync(
                item => item.OrganisationId == request.OrganisationId
                    && item.Id == request.PurchaseOrderId.Value
                    && item.SupplierId == request.SupplierId,
                cancellationToken);
            if (purchaseOrder is null)
            {
                throw new InvalidOperationException("Purchase order does not exist for the supplier.");
            }
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var receipt = new GoodsReceipt(
            Guid.NewGuid(),
            request.OrganisationId,
            request.BranchId,
            request.WarehouseId,
            request.SupplierId,
            request.PurchaseOrderId,
            request.ReceiptNumber,
            request.ReceiptDateUtc);
        var items = request.Items.Select(item => new GoodsReceiptItem(
            Guid.NewGuid(),
            request.OrganisationId,
            receipt.Id,
            item.ProductId,
            item.Description,
            item.Quantity)).ToList();

        _dbContext.GoodsReceipts.Add(receipt);
        _dbContext.GoodsReceiptItems.AddRange(items);
        foreach (var item in items)
        {
            await ApplyPurchaseReceiptAsync(request.OrganisationId, request.BranchId, request.WarehouseId, receipt.Id, item, cancellationToken);
        }

        purchaseOrder?.MarkReceived();
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ToResponse(receipt, items);
    }

    public async Task<SupplierInvoiceResponse> CreateSupplierInvoiceAsync(
        CreateSupplierInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("Supplier invoice must contain at least one item.");
        }

        await EnsureSupplierAsync(request.OrganisationId, request.SupplierId, cancellationToken);
        await EnsureBranchAsync(request.OrganisationId, request.BranchId, cancellationToken);
        await EnsureProductsAsync(request.OrganisationId, request.Items.Select(item => item.ProductId), cancellationToken);
        await EnsureUniqueAsync(
            _dbContext.SupplierInvoices.AnyAsync(
                invoice => invoice.OrganisationId == request.OrganisationId
                    && invoice.SupplierId == request.SupplierId
                    && invoice.InvoiceNumber == request.InvoiceNumber.Trim().ToUpperInvariant(),
                cancellationToken),
            "Supplier invoice number already exists for the supplier.");

        var subtotal = request.Items.Sum(item => item.Quantity * item.UnitCost);
        var taxTotal = request.Items.Sum(item => item.TaxAmount);
        var invoice = new SupplierInvoice(
            Guid.NewGuid(),
            request.OrganisationId,
            request.BranchId,
            request.SupplierId,
            request.PurchaseOrderId,
            request.InvoiceNumber,
            request.InvoiceDateUtc,
            subtotal,
            taxTotal,
            subtotal + taxTotal);

        _dbContext.SupplierInvoices.Add(invoice);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(invoice);
    }

    private async Task ApplyPurchaseReceiptAsync(
        Guid organisationId,
        Guid branchId,
        Guid warehouseId,
        Guid receiptId,
        GoodsReceiptItem item,
        CancellationToken cancellationToken)
    {
        var balance = await _dbContext.StockBalances.SingleOrDefaultAsync(
            balance => balance.OrganisationId == organisationId
                && balance.WarehouseId == warehouseId
                && balance.ProductId == item.ProductId,
            cancellationToken);
        if (balance is null)
        {
            balance = new StockBalance(Guid.NewGuid(), organisationId, warehouseId, item.ProductId);
            _dbContext.StockBalances.Add(balance);
        }

        var movement = new StockMovement(
            Guid.NewGuid(),
            organisationId,
            branchId,
            warehouseId,
            item.ProductId,
            StockMovementType.PurchaseReceipt,
            item.Quantity,
            "GoodsReceipt",
            receiptId,
            "Goods receipt stock in",
            DateTimeOffset.UtcNow);
        balance.Apply(movement.SignedQuantity);
        _dbContext.StockMovements.Add(movement);
    }

    private async Task EnsureSupplierAsync(Guid organisationId, Guid supplierId, CancellationToken cancellationToken)
    {
        var supplier = await _dbContext.Suppliers.SingleOrDefaultAsync(
            item => item.OrganisationId == organisationId && item.Id == supplierId,
            cancellationToken);
        if (supplier is null)
        {
            throw new InvalidOperationException("Supplier does not exist for the organisation.");
        }

        if (supplier.Status != SupplierStatus.Active)
        {
            throw new InvalidOperationException("Inactive suppliers cannot be used for purchasing.");
        }
    }

    private async Task EnsureBranchAsync(Guid organisationId, Guid branchId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Branches.AnyAsync(
            item => item.OrganisationId == organisationId && item.Id == branchId,
            cancellationToken);
        if (!exists)
        {
            throw new InvalidOperationException("Branch does not exist for the organisation.");
        }
    }

    private async Task EnsureProductsAsync(Guid organisationId, IEnumerable<Guid> productIds, CancellationToken cancellationToken)
    {
        var ids = productIds.Distinct().ToArray();
        var count = await _dbContext.Products.CountAsync(
            item => item.OrganisationId == organisationId && ids.Contains(item.Id) && item.Status == ProductStatus.Active,
            cancellationToken);
        if (count != ids.Length)
        {
            throw new InvalidOperationException("One or more products do not exist for the organisation.");
        }
    }

    private static async Task EnsureUniqueAsync(Task<bool> existsTask, string message)
    {
        if (await existsTask)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static List<PurchaseOrderItem> CreateOrderItems(
        Guid organisationId,
        Guid orderId,
        IReadOnlyCollection<PurchaseLineRequest> requests)
    {
        return requests.Select(item =>
        {
            var subtotal = item.Quantity * item.UnitCost;
            return new PurchaseOrderItem(
                Guid.NewGuid(),
                organisationId,
                orderId,
                item.ProductId,
                item.Description,
                item.Quantity,
                item.UnitCost,
                item.TaxAmount,
                subtotal + item.TaxAmount);
        }).ToList();
    }

    private static PurchaseOrderResponse ToResponse(PurchaseOrder order, IReadOnlyCollection<PurchaseOrderItem> items)
    {
        return new PurchaseOrderResponse(
            order.Id,
            order.OrganisationId,
            order.BranchId,
            order.SupplierId,
            order.OrderNumber,
            order.OrderDateUtc,
            order.Subtotal,
            order.TaxTotal,
            order.GrandTotal,
            order.Status.ToString(),
            items.Select(ToResponse).ToList());
    }

    private static PurchaseOrderItemResponse ToResponse(PurchaseOrderItem item)
    {
        return new PurchaseOrderItemResponse(
            item.Id,
            item.ProductId,
            item.Description,
            item.Quantity,
            item.UnitCost,
            item.TaxAmount,
            item.LineTotal);
    }

    private static GoodsReceiptResponse ToResponse(GoodsReceipt receipt, IReadOnlyCollection<GoodsReceiptItem> items)
    {
        return new GoodsReceiptResponse(
            receipt.Id,
            receipt.OrganisationId,
            receipt.BranchId,
            receipt.WarehouseId,
            receipt.SupplierId,
            receipt.PurchaseOrderId,
            receipt.ReceiptNumber,
            receipt.ReceiptDateUtc,
            items.Select(item => new GoodsReceiptItemResponse(item.Id, item.ProductId, item.Description, item.Quantity)).ToList());
    }

    private static SupplierInvoiceResponse ToResponse(SupplierInvoice invoice)
    {
        return new SupplierInvoiceResponse(
            invoice.Id,
            invoice.OrganisationId,
            invoice.BranchId,
            invoice.SupplierId,
            invoice.PurchaseOrderId,
            invoice.InvoiceNumber,
            invoice.InvoiceDateUtc,
            invoice.Subtotal,
            invoice.TaxTotal,
            invoice.GrandTotal,
            invoice.Status.ToString());
    }
}
