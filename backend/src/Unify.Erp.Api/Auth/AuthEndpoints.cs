using System.Security.Claims;
using Unify.Erp.Application.Auth;
using Unify.Erp.Contracts.Auth;

namespace Unify.Erp.Api.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/auth")
            .WithTags("Authentication");

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("Login");

        group.MapPost("/refresh", RefreshAsync)
            .AllowAnonymous()
            .WithName("RefreshToken");

        group.MapGet("/me", GetCurrentUser)
            .RequireAuthorization()
            .WithName("GetCurrentUser");

        group.MapGet("/sessions", ListSessionsAsync)
            .RequireAuthorization()
            .WithName("ListAuthSessions");

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout");

        group.MapPost("/logout-all", LogoutAllAsync)
            .RequireAuthorization()
            .WithName("LogoutAll");

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new { code = "auth.invalid_request" });
        }

        var result = await authenticationService.LoginAsync(request, cancellationToken);

        return ToHttpResult(result);
    }

    private static async Task<IResult> RefreshAsync(
        RefreshTokenRequest request,
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Results.BadRequest(new { code = "auth.invalid_request" });
        }

        var result = await authenticationService.RefreshAsync(request, cancellationToken);

        return ToHttpResult(result);
    }

    private static IResult ToHttpResult(AuthenticationResult result)
    {
        if (result.Succeeded && result.Tokens is not null)
        {
            return Results.Ok(result.Tokens);
        }

        return result.Error switch
        {
            AuthenticationError.DisabledUser => Results.Forbid(),
            AuthenticationError.ExpiredRefreshToken => Results.Unauthorized(),
            AuthenticationError.ReusedRefreshToken => Results.Unauthorized(),
            _ => Results.Unauthorized()
        };
    }

    private static IResult GetCurrentUser(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var email = user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        return Results.Ok(new CurrentUserResponse(userId.Value, email));
    }

    private static async Task<IResult> ListSessionsAsync(
        ClaimsPrincipal user,
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var sessions = await authenticationService.ListSessionsAsync(userId.Value, cancellationToken);

        return Results.Ok(sessions);
    }

    private static async Task<IResult> LogoutAsync(
        LogoutRequest request,
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Results.BadRequest(new { code = "auth.invalid_request" });
        }

        await authenticationService.LogoutAsync(request.RefreshToken, cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> LogoutAllAsync(
        ClaimsPrincipal user,
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        await authenticationService.LogoutAllAsync(userId.Value, cancellationToken);

        return Results.NoContent();
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var rawUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(rawUserId, out var userId) ? userId : null;
    }
}
