using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Unify.Erp.Application.Common;
using Unify.Erp.Domain.Audit;
using Unify.Erp.Domain.Common;

namespace Unify.Erp.Infrastructure.Persistence;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly IExecutionContext _executionContext;

    public AuditSaveChangesInterceptor(IExecutionContext executionContext)
    {
        _executionContext = executionContext;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AddAuditEntries(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddAuditEntries(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditEntries(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        var entries = dbContext.ChangeTracker.Entries<Entity>()
            .Where(entry => entry.Entity is not AuditEntry)
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(CreateAuditEntry)
            .OfType<AuditEntry>()
            .ToList();

        if (entries.Count == 0)
        {
            return;
        }

        dbContext.Set<AuditEntry>().AddRange(entries);
    }

    private AuditEntry? CreateAuditEntry(EntityEntry<Entity> entry)
    {
        var operation = entry.State switch
        {
            EntityState.Added => AuditOperation.Created,
            EntityState.Modified => AuditOperation.Updated,
            EntityState.Deleted => AuditOperation.Deleted,
            _ => throw new InvalidOperationException("Unsupported audit operation.")
        };

        var oldValues = new Dictionary<string, object?>(StringComparer.Ordinal);
        var newValues = new Dictionary<string, object?>(StringComparer.Ordinal);
        var changedProperties = new List<string>();

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsShadowProperty())
            {
                continue;
            }

            var propertyName = property.Metadata.Name;

            if (entry.State == EntityState.Added)
            {
                newValues[propertyName] = property.CurrentValue;
                changedProperties.Add(propertyName);
                continue;
            }

            if (entry.State == EntityState.Deleted)
            {
                oldValues[propertyName] = property.OriginalValue;
                changedProperties.Add(propertyName);
                continue;
            }

            if (!property.IsModified)
            {
                continue;
            }

            oldValues[propertyName] = property.OriginalValue;
            newValues[propertyName] = property.CurrentValue;
            changedProperties.Add(propertyName);
        }

        if (changedProperties.Count == 0)
        {
            return null;
        }

        var organisationId = entry.Entity is TenantEntity tenantEntity ? tenantEntity.OrganisationId : (Guid?)null;

        return new AuditEntry(
            Guid.NewGuid(),
            organisationId,
            _executionContext.UserId,
            _executionContext.UserEmail,
            _executionContext.CorrelationId,
            entry.Metadata.ClrType.Name,
            GetEntityId(entry),
            operation,
            Serialize(changedProperties.OrderBy(propertyName => propertyName, StringComparer.Ordinal)),
            oldValues.Count == 0 ? null : Serialize(oldValues),
            newValues.Count == 0 ? null : Serialize(newValues),
            DateTimeOffset.UtcNow);
    }

    private static string GetEntityId(EntityEntry entry)
    {
        var key = entry.Properties.SingleOrDefault(property => property.Metadata.IsPrimaryKey());

        return key?.CurrentValue?.ToString() ?? string.Empty;
    }

    private static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }
}
