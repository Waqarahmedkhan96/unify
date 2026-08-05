using Microsoft.EntityFrameworkCore;
using Unify.Erp.Application.Platform;
using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Platform;
using Unify.Erp.Domain.Branches;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Platform;

public sealed class BranchService : IBranchService
{
    private readonly ApplicationDbContext _dbContext;

    public BranchService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BranchResponse> CreateAsync(CreateBranchRequest request, CancellationToken cancellationToken)
    {
        var organisationExists = await _dbContext.Organisations
            .AnyAsync(organisation => organisation.Id == request.OrganisationId, cancellationToken);

        if (!organisationExists)
        {
            throw new InvalidOperationException("Organisation does not exist.");
        }

        var branch = new Branch(
            Guid.NewGuid(),
            request.OrganisationId,
            request.Code,
            request.Name,
            request.Timezone);

        _dbContext.Branches.Add(branch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(branch);
    }

    public async Task<PagedResponse<BranchResponse>> ListByOrganisationAsync(
        Guid organisationId,
        PagedRequest request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.NormalizedPageNumber;
        var pageSize = request.NormalizedPageSize;
        var query = _dbContext.Branches
            .AsNoTracking()
            .Where(branch => branch.OrganisationId == organisationId);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(branch => branch.Code)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(branch => ToResponse(branch))
            .ToListAsync(cancellationToken);

        return new PagedResponse<BranchResponse>(items, pageNumber, pageSize, totalCount);
    }

    private static BranchResponse ToResponse(Branch branch)
    {
        return new BranchResponse(
            branch.Id,
            branch.OrganisationId,
            branch.Code,
            branch.Name,
            branch.Timezone,
            branch.Status.ToString());
    }
}
