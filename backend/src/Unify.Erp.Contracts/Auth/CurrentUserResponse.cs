namespace Unify.Erp.Contracts.Auth;

public sealed record CurrentUserResponse(
    Guid UserId,
    string Email);
