namespace Unify.Erp.Infrastructure.Auth;

public sealed class PasswordResetDeliveryOptions
{
    public const string SectionName = "PasswordReset";

    public string FrontendBaseUrl { get; init; } = string.Empty;

    public string SenderEmail { get; init; } = string.Empty;

    public string SmtpHost { get; init; } = string.Empty;

    public int SmtpPort { get; init; } = 587;

    public bool SmtpEnableSsl { get; init; } = true;

    public string SmtpUsername { get; init; } = string.Empty;

    public string SmtpPassword { get; init; } = string.Empty;
}
