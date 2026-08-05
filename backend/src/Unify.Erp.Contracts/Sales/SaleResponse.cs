namespace Unify.Erp.Contracts.Sales;

public sealed record SaleResponse(
    Guid Id,
    Guid OrganisationId,
    Guid BranchId,
    Guid WarehouseId,
    Guid CustomerId,
    string InvoiceNumber,
    DateTimeOffset SaleDateUtc,
    decimal Subtotal,
    decimal DiscountTotal,
    decimal TaxTotal,
    decimal GrandTotal,
    string Status,
    IReadOnlyCollection<SaleItemResponse> Items);
