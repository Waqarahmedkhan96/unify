namespace Unify.Erp.Contracts.Audit;

public sealed record AuditEntryResponse(
    Guid Id,
    Guid? OrganisationId,
    Guid? UserId,
    string? UserEmail,
    string? CorrelationId,
    string EntityName,
    string EntityId,
    string Operation,
    string? ChangedProperties,
    string? OldValues,
    string? NewValues,
    DateTimeOffset OccurredAtUtc);
