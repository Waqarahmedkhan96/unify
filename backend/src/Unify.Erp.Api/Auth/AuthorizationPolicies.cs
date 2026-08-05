using Unify.Erp.Contracts.Auth;

namespace Unify.Erp.Api.Auth;

public static class AuthorizationPolicies
{
    public static IServiceCollection AddPermissionPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            foreach (var permission in PermissionNames.All)
            {
                options.AddPolicy(permission, policy =>
                    policy.RequireAuthenticatedUser().RequireClaim(PermissionNames.ClaimType, permission));
            }
        });

        return services;
    }
}
