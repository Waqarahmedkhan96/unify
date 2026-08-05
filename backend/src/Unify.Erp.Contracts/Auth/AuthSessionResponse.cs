namespace Unify.Erp.Contracts.Auth;

public sealed record AuthSessionResponse(
    Guid Id,
    Guid? OrganisationId,
    Guid? DeviceId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    bool IsActive);
