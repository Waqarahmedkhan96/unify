using System.Security.Claims;
using Unify.Erp.Application.Common;

namespace Unify.Erp.Api.Common;

public sealed class HttpExecutionContext : IExecutionContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpExecutionContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var rawUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(rawUserId, out var userId) ? userId : null;
        }
    }

    public string? UserEmail => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);

    public string? CorrelationId => _httpContextAccessor.HttpContext?.TraceIdentifier;
}
