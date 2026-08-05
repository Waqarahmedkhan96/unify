namespace Unify.Erp.Contracts.Platform;

public sealed record CreateWarehouseRequest(
    Guid OrganisationId,
    Guid BranchId,
    string Code,
    string Name);
