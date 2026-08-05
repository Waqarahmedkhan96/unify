namespace Unify.Erp.Contracts.Platform;

public sealed record CreateOrganisationRequest(
    string LegalName,
    string DisplayName,
    string BaseCurrency,
    string Timezone);
