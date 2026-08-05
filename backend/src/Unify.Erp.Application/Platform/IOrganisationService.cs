using Unify.Erp.Contracts.Common;
using Unify.Erp.Contracts.Platform;

namespace Unify.Erp.Application.Platform;

public interface IOrganisationService
{
    Task<OrganisationResponse> CreateAsync(CreateOrganisationRequest request, CancellationToken cancellationToken);

    Task<PagedResponse<OrganisationResponse>> ListAsync(PagedRequest request, CancellationToken cancellationToken);
}
