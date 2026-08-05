namespace Unify.Erp.Contracts.Purchasing;

public sealed record SupplierInvoiceResponse(
    Guid Id,
    Guid OrganisationId,
    Guid BranchId,
    Guid SupplierId,
    Guid? PurchaseOrderId,
    string InvoiceNumber,
    DateTimeOffset InvoiceDateUtc,
    decimal Subtotal,
    decimal TaxTotal,
    decimal GrandTotal,
    string Status);
