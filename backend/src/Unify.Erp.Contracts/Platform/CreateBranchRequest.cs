namespace Unify.Erp.Contracts.Platform;

public sealed record CreateBranchRequest(
    Guid OrganisationId,
    string Code,
    string Name,
    string Timezone);
