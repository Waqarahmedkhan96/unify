namespace Unify.Erp.Contracts.Purchasing;

public sealed record CreatePurchaseOrderRequest(
    Guid OrganisationId,
    Guid BranchId,
    Guid SupplierId,
    string OrderNumber,
    DateTimeOffset OrderDateUtc,
    IReadOnlyCollection<PurchaseLineRequest> Items);
