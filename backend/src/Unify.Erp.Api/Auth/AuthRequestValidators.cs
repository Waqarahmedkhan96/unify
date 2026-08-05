using Unify.Erp.Api.Common;
using Unify.Erp.Contracts.Auth;

namespace Unify.Erp.Api.Auth;

public static class AuthRequestValidators
{
    public static ValidationResult Validate(this LoginRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            result.Add(nameof(request.Email), "Value is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            result.Add(nameof(request.Password), "Value is required.");
        }

        return result;
    }

    public static ValidationResult Validate(this RefreshTokenRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            result.Add(nameof(request.RefreshToken), "Value is required.");
        }

        return result;
    }

    public static ValidationResult Validate(this LogoutRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            result.Add(nameof(request.RefreshToken), "Value is required.");
        }

        return result;
    }

    public static ValidationResult Validate(this ForgotPasswordRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            result.Add(nameof(request.Email), "Value is required.");
        }

        return result;
    }

    public static ValidationResult Validate(this ResetPasswordRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            result.Add(nameof(request.Email), "Value is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ResetToken))
        {
            result.Add(nameof(request.ResetToken), "Value is required.");
        }

        ValidateNewPassword(result, request.NewPassword);

        return result;
    }

    public static ValidationResult Validate(this ChangePasswordRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            result.Add(nameof(request.CurrentPassword), "Value is required.");
        }

        ValidateNewPassword(result, request.NewPassword);

        return result;
    }

    private static void ValidateNewPassword(ValidationResult result, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            result.Add("NewPassword", "Value is required.");
            return;
        }

        if (password.Length < 12)
        {
            result.Add("NewPassword", "Value must be at least 12 characters.");
        }
    }
}
