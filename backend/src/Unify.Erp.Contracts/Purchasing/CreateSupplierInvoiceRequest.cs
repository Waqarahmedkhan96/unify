namespace Unify.Erp.Contracts.Purchasing;

public sealed record CreateSupplierInvoiceRequest(
    Guid OrganisationId,
    Guid BranchId,
    Guid SupplierId,
    Guid? PurchaseOrderId,
    string InvoiceNumber,
    DateTimeOffset InvoiceDateUtc,
    IReadOnlyCollection<PurchaseLineRequest> Items);
