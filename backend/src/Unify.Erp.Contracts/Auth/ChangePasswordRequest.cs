namespace Unify.Erp.Contracts.Auth;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);
