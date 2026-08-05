using Microsoft.EntityFrameworkCore;
using Unify.Erp.Application.Audit;
using Unify.Erp.Contracts.Audit;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Domain.Audit;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Audit;

public sealed class AuditService : IAuditService
{
    private readonly ApplicationDbContext _dbContext;

    public AuditService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResponse<AuditEntryResponse>> ListAsync(
        ListAuditEntriesRequest request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.Page.NormalizedPageNumber;
        var pageSize = request.Page.NormalizedPageSize;
        var query = _dbContext.AuditEntries.AsNoTracking();

        if (request.OrganisationId.HasValue)
        {
            query = query.Where(entry => entry.OrganisationId == request.OrganisationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityName))
        {
            var entityName = request.EntityName.Trim();
            query = query.Where(entry => entry.EntityName == entityName);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityId))
        {
            var entityId = request.EntityId.Trim();
            query = query.Where(entry => entry.EntityId == entityId);
        }

        if (request.FromUtc.HasValue)
        {
            query = query.Where(entry => entry.OccurredAtUtc >= request.FromUtc.Value);
        }

        if (request.ToUtc.HasValue)
        {
            query = query.Where(entry => entry.OccurredAtUtc <= request.ToUtc.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(entry => entry.OccurredAtUtc)
            .ThenBy(entry => entry.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(entry => ToResponse(entry))
            .ToListAsync(cancellationToken);

        return new PagedResponse<AuditEntryResponse>(items, pageNumber, pageSize, totalCount);
    }

    private static AuditEntryResponse ToResponse(AuditEntry entry)
    {
        return new AuditEntryResponse(
            entry.Id,
            entry.OrganisationId,
            entry.UserId,
            entry.UserEmail,
            entry.CorrelationId,
            entry.EntityName,
            entry.EntityId,
            entry.Operation.ToString(),
            entry.ChangedProperties,
            entry.OldValues,
            entry.NewValues,
            entry.OccurredAtUtc);
    }
}
