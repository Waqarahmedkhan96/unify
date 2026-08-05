namespace Unify.Erp.Contracts.Reports;

public sealed record SalesReportResponse(
    Guid OrganisationId,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    Guid? CustomerId,
    Guid? ProductId,
    int InvoiceCount,
    decimal Quantity,
    decimal Subtotal,
    decimal DiscountTotal,
    decimal TaxTotal,
    decimal GrandTotal,
    IReadOnlyCollection<SalesReportProductRow> Products,
    IReadOnlyCollection<SalesReportInvoiceRow> Invoices);

public sealed record SalesReportProductRow(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    decimal Quantity,
    decimal SalesTotal);

public sealed record SalesReportInvoiceRow(
    Guid SaleId,
    string InvoiceNumber,
    DateTimeOffset SaleDateUtc,
    Guid CustomerId,
    string CustomerName,
    decimal GrandTotal);
