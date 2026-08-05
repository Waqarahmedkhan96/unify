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
}
