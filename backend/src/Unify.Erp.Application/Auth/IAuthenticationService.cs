using Unify.Erp.Contracts.Auth;

namespace Unify.Erp.Application.Auth;

public interface IAuthenticationService
{
    Task<AuthenticationResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<AuthenticationResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);

    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);

    Task LogoutAllAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AuthSessionResponse>> ListSessionsAsync(Guid userId, CancellationToken cancellationToken);

    Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);

    Task<PasswordOperationResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);

    Task<PasswordOperationResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken);
}
