namespace Unify.Erp.Contracts.Auth;

public sealed record ResetPasswordRequest(
    string Email,
    string ResetToken,
    string NewPassword);
