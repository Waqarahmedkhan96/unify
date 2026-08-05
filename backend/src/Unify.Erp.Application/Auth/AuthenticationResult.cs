using Unify.Erp.Contracts.Auth;

namespace Unify.Erp.Application.Auth;

public sealed record AuthenticationResult(
    bool Succeeded,
    AuthTokenResponse? Tokens,
    AuthenticationError Error)
{
    public static AuthenticationResult Success(AuthTokenResponse tokens)
    {
        return new AuthenticationResult(true, tokens, AuthenticationError.None);
    }

    public static AuthenticationResult Failure(AuthenticationError error)
    {
        return new AuthenticationResult(false, null, error);
    }
}
