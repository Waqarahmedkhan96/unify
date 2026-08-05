namespace Unify.Erp.Contracts.Sales;

public sealed record CreateSaleRequest(
    Guid OrganisationId,
    Guid BranchId,
    Guid WarehouseId,
    Guid CustomerId,
    string InvoiceNumber,
    DateTimeOffset SaleDateUtc,
    IReadOnlyCollection<CreateSaleItemRequest> Items);
