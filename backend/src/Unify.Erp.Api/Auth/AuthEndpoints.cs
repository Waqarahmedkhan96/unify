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
}
