using Unify.Erp.Domain.Common;

namespace Unify.Erp.Domain.Audit;

public sealed class AuditEntry : Entity
{
    public AuditEntry(
        Guid id,
        Guid? organisationId,
        Guid? userId,
        string? userEmail,
        string? correlationId,
        string entityName,
        string entityId,
        AuditOperation operation,
        string? changedProperties,
        string? oldValues,
        string? newValues,
        DateTimeOffset occurredAtUtc)
        : base(id)
    {
        OrganisationId = organisationId;
        UserId = userId;
        UserEmail = Guard.OptionalText(userEmail, nameof(userEmail), 254);
        CorrelationId = Guard.OptionalText(correlationId, nameof(correlationId), 128);
        EntityName = Guard.RequiredText(entityName, nameof(entityName), 160);
        EntityId = Guard.RequiredText(entityId, nameof(entityId), 80);
        Operation = operation;
        ChangedProperties = changedProperties;
        OldValues = oldValues;
        NewValues = newValues;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid? OrganisationId { get; }

    public Guid? UserId { get; }

    public string? UserEmail { get; }

    public string? CorrelationId { get; }

    public string EntityName { get; }

    public string EntityId { get; }

    public AuditOperation Operation { get; }

    public string? ChangedProperties { get; }

    public string? OldValues { get; }

    public string? NewValues { get; }

    public DateTimeOffset OccurredAtUtc { get; }
}
