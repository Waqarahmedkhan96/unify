namespace Unify.Erp.Contracts.Purchasing;

public sealed record PurchaseOrderResponse(
    Guid Id,
    Guid OrganisationId,
    Guid BranchId,
    Guid SupplierId,
    string OrderNumber,
    DateTimeOffset OrderDateUtc,
    decimal Subtotal,
    decimal TaxTotal,
    decimal GrandTotal,
    string Status,
    IReadOnlyCollection<PurchaseOrderItemResponse> Items);
