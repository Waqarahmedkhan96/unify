using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Unify.Erp.Api.Auth;
using Unify.Erp.Api.Accounting;
using Unify.Erp.Api.Common;
using Unify.Erp.Api.Customers;
using Unify.Erp.Api.Inventory;
using Unify.Erp.Api.Payments;
using Unify.Erp.Api.Platform;
using Unify.Erp.Api.Products;
using Unify.Erp.Api.Purchasing;
using Unify.Erp.Api.Sales;
using Unify.Erp.Api.Suppliers;
using Unify.Erp.Application;
using Unify.Erp.Application.Common;
using Unify.Erp.Contracts.System;
using Unify.Erp.Infrastructure;
using Unify.Erp.Infrastructure.Auth;
using Unify.Erp.Infrastructure.Deployment;
using Unify.Erp.Infrastructure.Seed;

const string ApiCorsPolicy = "UnifyApiCors";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IExecutionContext, HttpExecutionContext>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

ProductionConfigurationValidator.Validate(builder.Configuration, builder.Environment.EnvironmentName);

var corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
var rateLimitingOptions = builder.Configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>()
    ?? new RateLimitingOptions();
var httpsOptions = builder.Configuration.GetSection(HttpsOptions.SectionName).Get<HttpsOptions>() ?? new HttpsOptions();

if (builder.Environment.IsProduction())
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    builder.Services.AddHsts(options =>
    {
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(Math.Max(1, httpsOptions.HstsDays));
    });
}

if (corsOptions.AllowedOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(
            ApiCorsPolicy,
            policy => policy
                .WithOrigins(corsOptions.AllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod());
    });
}

if (rateLimitingOptions.Enabled)
{
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var partitionKey = context.User.Identity?.Name
                ?? context.Connection.RemoteIpAddress?.ToString()
                ?? "anonymous";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey,
                _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = Math.Max(1, rateLimitingOptions.PermitLimit),
                    QueueLimit = 0,
                    Window = TimeSpan.FromSeconds(Math.Max(1, rateLimitingOptions.WindowSeconds))
                });
        });
    });
}

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

    builder.Services.AddPermissionPolicies();
}

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseForwardedHeaders();
}

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

if (corsOptions.AllowedOrigins.Length > 0)
{
    app.UseCors(ApiCorsPolicy);
}

if (app.Environment.IsProduction())
{
    if (httpsOptions.RequireHttps)
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }
}

if (app.Environment.IsDevelopment())
{
    await DevelopmentDataSeeder.InitializeAsync(app.Services, CancellationToken.None);
}
else
{
    await BootstrapAdminSeeder.InitializeAsync(app.Services, CancellationToken.None);
}

if (!string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
    app.UseAuthentication();
}

if (rateLimitingOptions.Enabled)
{
    app.UseRateLimiter();
}

if (!string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
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
app.MapPurchasingEndpoints();
app.MapAccountingEndpoints();

app.Run();

public partial class Program;
