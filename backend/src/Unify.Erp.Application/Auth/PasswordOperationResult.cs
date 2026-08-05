namespace Unify.Erp.Application.Auth;

public sealed record PasswordOperationResult(bool Succeeded, IReadOnlyCollection<string> Errors)
{
    public static PasswordOperationResult Success() => new(true, []);

    public static PasswordOperationResult Failure(params string[] errors) => new(false, errors);
}
