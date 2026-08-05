namespace Unify.Erp.Contracts.Auth;

public sealed record LoginRequest(
    string Email,
    string Password,
    Guid? OrganisationId,
    Guid? DeviceId);
