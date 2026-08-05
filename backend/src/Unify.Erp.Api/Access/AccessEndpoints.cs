using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Unify.Erp.Contracts.Access;
using Unify.Erp.Contracts.Auth;
using Unify.Erp.Infrastructure.Identity;
using Unify.Erp.Infrastructure.Persistence;

namespace Unify.Erp.Api.Access;

public static class AccessEndpoints
{
    private static readonly IReadOnlyDictionary<string, string> PermissionDescriptions = new Dictionary<string, string>
    {
        [PermissionNames.PlatformManage] = "Manage platform setup, users, permissions, branches, warehouses, and audit settings.",
        [PermissionNames.CustomersManage] = "Create, view, and manage customers.",
        [PermissionNames.SuppliersManage] = "Create, view, and manage suppliers.",
        [PermissionNames.ProductsManage] = "Create, view, and manage product catalog data.",
        [PermissionNames.InventoryManage] = "Manage warehouse stock, transfers, movements, and adjustments.",
        [PermissionNames.SalesManage] = "Create and view sales invoices and sales operations.",
        [PermissionNames.PaymentsManage] = "Manage customer payments, balances, and ledgers.",
        [PermissionNames.PurchasingManage] = "Manage purchase orders, goods receipts, and supplier invoices.",
        [PermissionNames.AccountingManage] = "Manage chart of accounts, fiscal periods, and journals."
    };

    public static IEndpointRouteBuilder MapAccessEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/access")
            .RequireAuthorization()
            .RequireAuthorization(PermissionNames.PlatformManage)
            .WithTags("Access");

        group.MapGet("/permissions", ListPermissions)
            .WithName("ListAccessPermissions");

        group.MapGet("/users", ListUsersAsync)
            .WithName("ListAccessUsers");

        group.MapPost("/users", CreateUserAsync)
            .WithName("CreateAccessUser");

        group.MapPut("/users/{userId:guid}/permissions", UpdateUserPermissionsAsync)
            .WithName("UpdateAccessUserPermissions");

        group.MapPost("/users/{userId:guid}/disable", DisableUserAsync)
            .WithName("DisableAccessUser");

        group.MapPost("/users/{userId:guid}/enable", EnableUserAsync)
            .WithName("EnableAccessUser");

        return endpoints;
    }

    private static IResult ListPermissions()
    {
        var response = PermissionNames.All
            .Select(permission => new AccessPermissionResponse(permission, PermissionDescriptions[permission]))
            .ToArray();

        return Results.Ok(response);
    }

    private static async Task<IResult> ListUsersAsync(
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var users = await userManager.Users
            .OrderBy(user => user.Email)
            .ToListAsync(cancellationToken);
        var response = new List<AccessUserResponse>(users.Count);

        foreach (var user in users)
        {
            response.Add(await ToResponseAsync(userManager, user));
        }

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateUserAsync(
        CreateAccessUserRequest request,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.DisplayName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new { code = "access.user_required" });
        }

        var permissions = NormalizePermissions(request.Permissions);
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim(),
            UserName = request.Email.Trim(),
            DisplayName = request.DisplayName.Trim(),
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return Results.BadRequest(new
            {
                code = "access.user_create_failed",
                errors = result.Errors.Select(error => error.Description).ToArray()
            });
        }

        await ReplacePermissionsAsync(userManager, user, permissions);
        var response = await ToResponseAsync(userManager, user);

        return Results.Created($"/api/v1/access/users/{user.Id}", response);
    }

    private static async Task<IResult> UpdateUserPermissionsAsync(
        Guid userId,
        UpdateUserPermissionsRequest request,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Results.NotFound();
        }

        var permissions = NormalizePermissions(request.Permissions);
        var currentClaims = await userManager.GetClaimsAsync(user);
        var currentlyAdmin = currentClaims.Any(claim =>
            claim.Type == PermissionNames.ClaimType &&
            claim.Value == PermissionNames.PlatformManage);
        var willRemainAdmin = permissions.Contains(PermissionNames.PlatformManage);

        if (currentlyAdmin && !willRemainAdmin && await IsLastActivePlatformAdminAsync(dbContext, userManager, user.Id, cancellationToken))
        {
            return Results.BadRequest(new { code = "access.last_platform_admin_required" });
        }

        await ReplacePermissionsAsync(userManager, user, permissions);

        return Results.Ok(await ToResponseAsync(userManager, user));
    }

    private static async Task<IResult> DisableUserAsync(
        Guid userId,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Results.NotFound();
        }

        var claims = await userManager.GetClaimsAsync(user);
        var isPlatformAdmin = claims.Any(claim =>
            claim.Type == PermissionNames.ClaimType &&
            claim.Value == PermissionNames.PlatformManage);

        if (isPlatformAdmin && await IsLastActivePlatformAdminAsync(dbContext, userManager, user.Id, cancellationToken))
        {
            return Results.BadRequest(new { code = "access.last_platform_admin_required" });
        }

        user.IsDisabled = true;
        await userManager.UpdateAsync(user);

        return Results.Ok(await ToResponseAsync(userManager, user));
    }

    private static async Task<IResult> EnableUserAsync(
        Guid userId,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Results.NotFound();
        }

        user.IsDisabled = false;
        await userManager.UpdateAsync(user);

        return Results.Ok(await ToResponseAsync(userManager, user));
    }

    private static IReadOnlyCollection<string> NormalizePermissions(IReadOnlyCollection<string>? permissions)
    {
        return (permissions ?? Array.Empty<string>())
            .Select(permission => permission.Trim().ToLowerInvariant())
            .Where(permission => PermissionNames.All.Contains(permission))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order()
            .ToArray();
    }

    private static async Task ReplacePermissionsAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        IReadOnlyCollection<string> permissions)
    {
        var existingClaims = await userManager.GetClaimsAsync(user);
        foreach (var claim in existingClaims.Where(claim => claim.Type == PermissionNames.ClaimType))
        {
            await userManager.RemoveClaimAsync(user, claim);
        }

        foreach (var permission in permissions)
        {
            await userManager.AddClaimAsync(user, new Claim(PermissionNames.ClaimType, permission));
        }
    }

    private static async Task<bool> IsLastActivePlatformAdminAsync(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        Guid targetUserId,
        CancellationToken cancellationToken)
    {
        var activeUsers = await dbContext.Users
            .Where(user => !user.IsDisabled && user.Id != targetUserId)
            .ToListAsync(cancellationToken);

        foreach (var user in activeUsers)
        {
            var claims = await userManager.GetClaimsAsync(user);
            if (claims.Any(claim => claim.Type == PermissionNames.ClaimType && claim.Value == PermissionNames.PlatformManage))
            {
                return false;
            }
        }

        return true;
    }

    private static async Task<AccessUserResponse> ToResponseAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user)
    {
        var permissions = (await userManager.GetClaimsAsync(user))
            .Where(claim => claim.Type == PermissionNames.ClaimType)
            .Select(claim => claim.Value)
            .Order()
            .ToArray();

        return new AccessUserResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.IsDisabled,
            permissions);
    }
}
