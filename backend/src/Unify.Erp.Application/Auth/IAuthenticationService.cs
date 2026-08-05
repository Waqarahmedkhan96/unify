using Unify.Erp.Contracts.Auth;

namespace Unify.Erp.Application.Auth;

public interface IAuthenticationService
{
    Task<AuthenticationResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<AuthenticationResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
}
