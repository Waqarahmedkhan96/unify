using Unify.Erp.Contracts.Common;

namespace Unify.Erp.Contracts.Audit;

public sealed record ListAuditEntriesRequest(
    Guid? OrganisationId,
    string? EntityName,
    string? EntityId,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    PagedRequest Page);
