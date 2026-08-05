using Microsoft.Extensions.Primitives;

namespace Unify.Erp.Api.Common;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out StringValues values)
            && !StringValues.IsNullOrEmpty(values))
        {
            context.TraceIdentifier = values.ToString();
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = context.TraceIdentifier;
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
