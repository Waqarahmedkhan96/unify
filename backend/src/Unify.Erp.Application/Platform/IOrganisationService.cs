using Unify.Erp.Contracts.Platform;

namespace Unify.Erp.Application.Platform;

public interface IOrganisationService
{
    Task<OrganisationResponse> CreateAsync(CreateOrganisationRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<OrganisationResponse>> ListAsync(CancellationToken cancellationToken);
}
