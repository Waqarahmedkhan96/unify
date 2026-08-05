using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Purchasing;

namespace Unify.Erp.Application.Purchasing;

public interface IPurchasingService
{
    Task<PurchaseOrderResponse> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request, CancellationToken cancellationToken);

    Task<PagedResponse<PurchaseOrderResponse>> ListPurchaseOrdersAsync(
        ListPurchasingDocumentsRequest request,
        CancellationToken cancellationToken);

    Task<GoodsReceiptResponse> CreateGoodsReceiptAsync(CreateGoodsReceiptRequest request, CancellationToken cancellationToken);

    Task<SupplierInvoiceResponse> CreateSupplierInvoiceAsync(
        CreateSupplierInvoiceRequest request,
        CancellationToken cancellationToken);
}
