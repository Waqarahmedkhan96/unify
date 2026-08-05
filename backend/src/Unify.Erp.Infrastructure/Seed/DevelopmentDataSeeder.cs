using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Unify.Erp.Contracts.Auth;
using Unify.Erp.Infrastructure.Identity;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Seed;

public static class DevelopmentDataSeeder
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var databaseOptions = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        var seedOptions = scope.ServiceProvider.GetRequiredService<IOptions<DevelopmentSeedOptions>>().Value;
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (databaseOptions.ApplyMigrationsOnStartup)
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        if (!seedOptions.Enabled)
        {
            return;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(seedOptions.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(seedOptions.Password);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var existingUser = await userManager.FindByEmailAsync(seedOptions.Email);

        if (existingUser is not null)
        {
            await EnsurePermissionsAsync(userManager, existingUser);
            return;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = seedOptions.Email,
            UserName = seedOptions.Email,
            DisplayName = seedOptions.DisplayName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, seedOptions.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
            throw new InvalidOperationException($"Development user seed failed: {errors}");
        }

        await EnsurePermissionsAsync(userManager, user);
    }

    private static async Task EnsurePermissionsAsync(UserManager<ApplicationUser> userManager, ApplicationUser user)
    {
        var claims = await userManager.GetClaimsAsync(user);
        var existingPermissions = claims
            .Where(claim => claim.Type == PermissionNames.ClaimType)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var permission in PermissionNames.All.Where(permission => !existingPermissions.Contains(permission)))
        {
            await userManager.AddClaimAsync(user, new Claim(PermissionNames.ClaimType, permission));
        }
    }
}
