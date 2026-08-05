using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Unify.Erp.Application.Auth;

namespace Unify.Erp.Infrastructure.Auth;

public sealed class PasswordResetNotificationService : IPasswordResetNotificationService
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<PasswordResetNotificationService> _logger;
    private readonly PasswordResetDeliveryOptions _options;

    public PasswordResetNotificationService(
        IHostEnvironment environment,
        IOptions<PasswordResetDeliveryOptions> options,
        ILogger<PasswordResetNotificationService> logger)
    {
        _environment = environment;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetAsync(
        string email,
        string resetToken,
        CancellationToken cancellationToken)
    {
        if (_environment.IsDevelopment() && string.IsNullOrWhiteSpace(_options.SmtpHost))
        {
            _logger.LogWarning(
                "Development password reset token generated for {Email}: {ResetToken}",
                email,
                resetToken);
            return;
        }

        using var message = new MailMessage(_options.SenderEmail, email)
        {
            Subject = "Reset your Unify ERP password",
            Body = BuildBody(email, resetToken)
        };

        using var smtpClient = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.SmtpEnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_options.SmtpUsername))
        {
            smtpClient.Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword);
        }

        await smtpClient.SendMailAsync(message, cancellationToken);
    }

    private string BuildBody(string email, string resetToken)
    {
        if (string.IsNullOrWhiteSpace(_options.FrontendBaseUrl))
        {
            return $"Use this reset token to reset your password: {resetToken}";
        }

        var resetUrl = $"{_options.FrontendBaseUrl.TrimEnd('/')}/reset-password"
            + $"?email={Uri.EscapeDataString(email)}"
            + $"&token={Uri.EscapeDataString(resetToken)}";

        return $"Use this secure link to reset your password: {resetUrl}";
    }
}
