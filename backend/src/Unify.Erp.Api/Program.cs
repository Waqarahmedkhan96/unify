using Unify.Erp.Application;
using Unify.Erp.Contracts.System;
using Unify.Erp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.MapGet("/api/v1/system/health", () =>
{
    var response = new HealthResponse("Healthy", "Unify ERP API", DateTimeOffset.UtcNow);

    return Results.Ok(response);
})
.WithName("GetSystemHealth");

app.Run();

public partial class Program;
