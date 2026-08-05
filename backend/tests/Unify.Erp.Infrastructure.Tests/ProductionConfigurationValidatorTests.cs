using Microsoft.Extensions.Configuration;
using Unify.Erp.Infrastructure.Deployment;

namespace Unify.Erp.Infrastructure.Tests;

public sealed class ProductionConfigurationValidatorTests
{
    [Fact]
    public void Validate_allows_non_production_empty_configuration()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>());

        ProductionConfigurationValidator.Validate(configuration, "Development");
    }

    [Fact]
    public void Validate_rejects_production_without_required_secrets()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Jwt:AccessTokenMinutes"] = "15",
            ["Jwt:RefreshTokenDays"] = "30",
            ["DevelopmentSeed:Enabled"] = "false"
        });

        var exception = Assert.Throws<InvalidOperationException>(
            () => ProductionConfigurationValidator.Validate(configuration, "Production"));

        Assert.Contains("ConnectionStrings:Default is required", exception.Message);
        Assert.Contains("Jwt:SigningKey is required", exception.Message);
    }

    [Fact]
    public void Validate_rejects_production_development_seed()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=db;Database=unify;Username=unify_app;Password=strong-password",
            ["Jwt:Issuer"] = "Unify.Erp",
            ["Jwt:Audience"] = "Unify.Erp.Client",
            ["Jwt:SigningKey"] = "production-signing-key-with-at-least-32-characters",
            ["Jwt:AccessTokenMinutes"] = "15",
            ["Jwt:RefreshTokenDays"] = "30",
            ["PasswordReset:FrontendBaseUrl"] = "https://app.example.com",
            ["PasswordReset:SenderEmail"] = "no-reply@example.com",
            ["PasswordReset:SmtpHost"] = "smtp.example.com",
            ["PasswordReset:SmtpPort"] = "587",
            ["DevelopmentSeed:Enabled"] = "true"
        });

        var exception = Assert.Throws<InvalidOperationException>(
            () => ProductionConfigurationValidator.Validate(configuration, "Production"));

        Assert.Contains("DevelopmentSeed:Enabled must be false", exception.Message);
    }

    [Fact]
    public void Validate_allows_complete_production_configuration()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=db;Database=unify;Username=unify_app;Password=strong-password",
            ["Jwt:Issuer"] = "Unify.Erp",
            ["Jwt:Audience"] = "Unify.Erp.Client",
            ["Jwt:SigningKey"] = "production-signing-key-with-at-least-32-characters",
            ["Jwt:AccessTokenMinutes"] = "15",
            ["Jwt:RefreshTokenDays"] = "30",
            ["PasswordReset:FrontendBaseUrl"] = "https://app.example.com",
            ["PasswordReset:SenderEmail"] = "no-reply@example.com",
            ["PasswordReset:SmtpHost"] = "smtp.example.com",
            ["PasswordReset:SmtpPort"] = "587",
            ["DevelopmentSeed:Enabled"] = "false"
        });

        ProductionConfigurationValidator.Validate(configuration, "Production");
    }

    [Fact]
    public void Validate_rejects_production_without_password_reset_delivery()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=db;Database=unify;Username=unify_app;Password=strong-password",
            ["Jwt:Issuer"] = "Unify.Erp",
            ["Jwt:Audience"] = "Unify.Erp.Client",
            ["Jwt:SigningKey"] = "production-signing-key-with-at-least-32-characters",
            ["Jwt:AccessTokenMinutes"] = "15",
            ["Jwt:RefreshTokenDays"] = "30",
            ["DevelopmentSeed:Enabled"] = "false"
        });

        var exception = Assert.Throws<InvalidOperationException>(
            () => ProductionConfigurationValidator.Validate(configuration, "Production"));

        Assert.Contains("PasswordReset:SmtpHost is required", exception.Message);
    }

    private static IConfiguration BuildConfiguration(IReadOnlyDictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
