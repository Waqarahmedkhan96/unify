namespace Unify.Erp.Contracts.Platform;

public sealed record WarehouseResponse(
    Guid Id,
    Guid OrganisationId,
    Guid BranchId,
    string Code,
    string Name,
    string Status);
