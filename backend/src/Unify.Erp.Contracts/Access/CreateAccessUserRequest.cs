namespace Unify.Erp.Contracts.Access;

public sealed record CreateAccessUserRequest(
    string Email,
    string DisplayName,
    string Password,
    IReadOnlyCollection<string> Permissions);
