namespace Unify.Erp.Contracts.Access;

public sealed record UpdateUserPermissionsRequest(IReadOnlyCollection<string> Permissions);
