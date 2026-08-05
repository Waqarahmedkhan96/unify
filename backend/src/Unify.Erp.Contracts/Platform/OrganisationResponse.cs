namespace Unify.Erp.Contracts.Platform;

public sealed record OrganisationResponse(
    Guid Id,
    string LegalName,
    string DisplayName,
    string BaseCurrency,
    string Timezone,
    string Status,
    DateTimeOffset CreatedAtUtc);
