using Unify.Erp.Contracts.Common;

namespace Unify.Erp.Contracts.Inventory;

public sealed record ListStockMovementsRequest(
    Guid OrganisationId,
    Guid? WarehouseId,
    Guid? ProductId,
    PagedRequest Page);
