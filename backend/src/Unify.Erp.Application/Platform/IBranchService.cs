using Unify.Erp.Contracts.Platform;

namespace Unify.Erp.Application.Platform;

public interface IBranchService
{
    Task<BranchResponse> CreateAsync(CreateBranchRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<BranchResponse>> ListByOrganisationAsync(Guid organisationId, CancellationToken cancellationToken);
}
