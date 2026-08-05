namespace Unify.Erp.Contracts.Access;

public sealed record AccessUserResponse(
    Guid Id,
    string Email,
    string DisplayName,
    bool IsDisabled,
    IReadOnlyCollection<string> Permissions);
