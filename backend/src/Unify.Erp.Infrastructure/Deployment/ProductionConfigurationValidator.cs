using Microsoft.Extensions.Configuration;
using Unify.Erp.Infrastructure.Auth;
using Unify.Erp.Infrastructure.Seed;

namespace Unify.Erp.Infrastructure.Deployment;

public static class ProductionConfigurationValidator
{
    private const int MinimumSigningKeyLength = 32;

    public static void Validate(IConfiguration configuration, string environmentName)
    {
        if (!string.Equals(environmentName, "Production", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var errors = new List<string>();
        var connectionString = configuration.GetConnectionString("Default");
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var seedOptions = configuration.GetSection(DevelopmentSeedOptions.SectionName).Get<DevelopmentSeedOptions>() ?? new DevelopmentSeedOptions();

        AddRequired(errors, "ConnectionStrings:Default", connectionString);
        AddRequired(errors, "Jwt:Issuer", jwtOptions.Issuer);
        AddRequired(errors, "Jwt:Audience", jwtOptions.Audience);
        AddRequired(errors, "Jwt:SigningKey", jwtOptions.SigningKey);

        if (!string.IsNullOrWhiteSpace(jwtOptions.SigningKey)
            && jwtOptions.SigningKey.Length < MinimumSigningKeyLength)
        {
            errors.Add($"Jwt:SigningKey must be at least {MinimumSigningKeyLength} characters.");
        }

        if (jwtOptions.AccessTokenMinutes < 1 || jwtOptions.AccessTokenMinutes > 60)
        {
            errors.Add("Jwt:AccessTokenMinutes must be between 1 and 60.");
        }

        if (jwtOptions.RefreshTokenDays < 1 || jwtOptions.RefreshTokenDays > 90)
        {
            errors.Add("Jwt:RefreshTokenDays must be between 1 and 90.");
        }

        if (seedOptions.Enabled)
        {
            errors.Add("DevelopmentSeed:Enabled must be false in Production.");
        }

        if (ContainsDevelopmentPlaceholder(connectionString) || ContainsDevelopmentPlaceholder(jwtOptions.SigningKey))
        {
            errors.Add("Production configuration contains development placeholder values.");
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Production configuration is invalid: " + string.Join(" ", errors));
        }
    }

    private static void AddRequired(List<string> errors, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{key} is required.");
        }
    }

    private static bool ContainsDevelopmentPlaceholder(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains("change_this", StringComparison.OrdinalIgnoreCase);
    }
}
