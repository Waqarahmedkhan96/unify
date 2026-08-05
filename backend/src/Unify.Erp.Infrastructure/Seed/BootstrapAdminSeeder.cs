using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Unify.Erp.Contracts.Auth;
using Unify.Erp.Infrastructure.Identity;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Infrastructure.Seed;

public static class BootstrapAdminSeeder
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<BootstrapAdminOptions>>().Value;

        if (!options.Enabled)
        {
            return;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(options.Email);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Password);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.DisplayName);

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userCount = await dbContext.Users.CountAsync(cancellationToken);
        if (userCount > 0)
        {
            return;
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = options.Email,
            UserName = options.Email,
            DisplayName = options.DisplayName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, options.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
            throw new InvalidOperationException($"Bootstrap admin creation failed: {errors}");
        }

        foreach (var permission in PermissionNames.All)
        {
            await userManager.AddClaimAsync(user, new Claim(PermissionNames.ClaimType, permission));
        }
    }
}
