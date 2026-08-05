using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Unify.Erp.Api.Auth;
using Unify.Erp.Api.Common;
using Unify.Erp.Api.Customers;
using Unify.Erp.Api.Inventory;
using Unify.Erp.Api.Payments;
using Unify.Erp.Api.Platform;
using Unify.Erp.Api.Products;
using Unify.Erp.Api.Sales;
using Unify.Erp.Api.Suppliers;
using Unify.Erp.Application;
using Unify.Erp.Contracts.System;
using Unify.Erp.Infrastructure;
using Unify.Erp.Infrastructure.Auth;
using Unify.Erp.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

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

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = feature?.Error;
        var isClientError = exception is ArgumentException
            or InvalidOperationException
            or BadHttpRequestException;

        context.Response.StatusCode = isClientError
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await Results.Problem(
            title: isClientError ? "Request could not be processed." : "An unexpected error occurred.",
            statusCode: context.Response.StatusCode,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = isClientError ? "common.invalid_request" : "common.unhandled_error",
                ["correlationId"] = context.TraceIdentifier
            }).ExecuteAsync(context);
    });
});

app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    await DevelopmentDataSeeder.InitializeAsync(app.Services, CancellationToken.None);
}

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
app.MapPlatformEndpoints();
app.MapCustomerEndpoints();
app.MapSupplierEndpoints();
app.MapProductCatalogEndpoints();
app.MapInventoryEndpoints();
app.MapSalesEndpoints();
app.MapPaymentEndpoints();

app.Run();

public partial class Program;
