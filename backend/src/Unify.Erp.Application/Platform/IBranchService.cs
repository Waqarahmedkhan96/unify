using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Platform;

namespace Unify.Erp.Application.Platform;

public interface IBranchService
{
    Task<BranchResponse> CreateAsync(CreateBranchRequest request, CancellationToken cancellationToken);

    Task<PagedResponse<BranchResponse>> ListByOrganisationAsync(
        Guid organisationId,
        PagedRequest request,
        CancellationToken cancellationToken);
}
