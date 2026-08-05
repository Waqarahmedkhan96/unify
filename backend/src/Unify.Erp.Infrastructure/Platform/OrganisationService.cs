using Microsoft.EntityFrameworkCore;
using Unify.Erp.Application.Platform;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Platform;
using Unify.Erp.Domain.Organisations;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Platform;

public sealed class OrganisationService : IOrganisationService
{
    private readonly ApplicationDbContext _dbContext;

    public OrganisationService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrganisationResponse> CreateAsync(
        CreateOrganisationRequest request,
        CancellationToken cancellationToken)
    {
        var organisation = new Organisation(
            Guid.NewGuid(),
            request.LegalName,
            request.DisplayName,
            request.BaseCurrency,
            request.Timezone);

        _dbContext.Organisations.Add(organisation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(organisation);
    }

    public async Task<PagedResponse<OrganisationResponse>> ListAsync(
        PagedRequest request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.NormalizedPageNumber;
        var pageSize = request.NormalizedPageSize;
        var query = _dbContext.Organisations.AsNoTracking();
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .AsNoTracking()
            .OrderBy(organisation => organisation.DisplayName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(organisation => ToResponse(organisation))
            .ToListAsync(cancellationToken);

        return new PagedResponse<OrganisationResponse>(items, pageNumber, pageSize, totalCount);
    }

    private static OrganisationResponse ToResponse(Organisation organisation)
    {
        return new OrganisationResponse(
            organisation.Id,
            organisation.LegalName,
            organisation.DisplayName,
            organisation.BaseCurrency,
            organisation.Timezone,
            organisation.Status.ToString(),
            organisation.CreatedAtUtc);
    }
}
