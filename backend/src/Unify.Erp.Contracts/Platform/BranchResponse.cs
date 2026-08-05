namespace Unify.Erp.Contracts.Platform;

public sealed record BranchResponse(
    Guid Id,
    Guid OrganisationId,
    string Code,
    string Name,
    string Timezone,
    string Status);
