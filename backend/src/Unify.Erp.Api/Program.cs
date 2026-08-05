using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Unify.Erp.Api.Auth;
using Unify.Erp.Application;
using Unify.Erp.Contracts.System;
using Unify.Erp.Infrastructure;
using Unify.Erp.Infrastructure.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (!string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

    builder.Services.AddAuthorization();
}

var app = builder.Build();

if (!string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.MapGet("/api/v1/system/health", () =>
{
    var response = new HealthResponse("Healthy", "Unify ERP API", DateTimeOffset.UtcNow);

    return Results.Ok(response);
})
.WithName("GetSystemHealth");

app.MapAuthEndpoints();

app.Run();

public partial class Program;
