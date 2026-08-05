namespace Unify.Erp.Application.Auth;

public interface IPasswordResetNotificationService
{
    Task SendPasswordResetAsync(
        string email,
        string resetToken,
        CancellationToken cancellationToken);
}
