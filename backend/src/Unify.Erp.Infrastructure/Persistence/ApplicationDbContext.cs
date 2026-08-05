using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Unify.Erp.Domain.Access;
using Unify.Erp.Domain.Branches;
using Unify.Erp.Domain.Devices;
using Unify.Erp.Domain.Organisations;
using Unify.Erp.Domain.Sessions;
using Unify.Erp.Domain.Users;
using Unify.Erp.Domain.Warehouses;
using Unify.Erp.Infrastructure.Auth;
using Unify.Erp.Infrastructure.Identity;

namespace Unify.Erp.Infrastructure.Persistence;

public sealed class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organisation> Organisations => Set<Organisation>();

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    public DbSet<PlatformUser> PlatformUsers => Set<PlatformUser>();

    public DbSet<UserOrganisationMembership> UserOrganisationMemberships => Set<UserOrganisationMembership>();

    public DbSet<Role> RolesCatalog => Set<Role>();

    public DbSet<Permission> PermissionsCatalog => Set<Permission>();

    public DbSet<Device> Devices => Set<Device>();

    public DbSet<DeviceSession> DeviceSessions => Set<DeviceSession>();

    public DbSet<RefreshTokenRecord> RefreshTokens => Set<RefreshTokenRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureIdentity(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    private static void ConfigureIdentity(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("identity_users");
            entity.Property(user => user.Id).HasColumnName("id");
            entity.Property(user => user.DisplayName).HasColumnName("display_name");
            entity.Property(user => user.IsDisabled).HasColumnName("is_disabled");
            entity.Property(user => user.UserName).HasColumnName("user_name");
            entity.Property(user => user.NormalizedUserName).HasColumnName("normalized_user_name");
            entity.Property(user => user.Email).HasColumnName("email");
            entity.Property(user => user.NormalizedEmail).HasColumnName("normalized_email");
            entity.Property(user => user.EmailConfirmed).HasColumnName("email_confirmed");
            entity.Property(user => user.PasswordHash).HasColumnName("password_hash");
            entity.Property(user => user.SecurityStamp).HasColumnName("security_stamp");
            entity.Property(user => user.ConcurrencyStamp).HasColumnName("concurrency_stamp");
            entity.Property(user => user.PhoneNumber).HasColumnName("phone_number");
            entity.Property(user => user.PhoneNumberConfirmed).HasColumnName("phone_number_confirmed");
            entity.Property(user => user.TwoFactorEnabled).HasColumnName("two_factor_enabled");
            entity.Property(user => user.LockoutEnd).HasColumnName("lockout_end");
            entity.Property(user => user.LockoutEnabled).HasColumnName("lockout_enabled");
            entity.Property(user => user.AccessFailedCount).HasColumnName("access_failed_count");
            entity.HasIndex(user => user.NormalizedEmail).HasDatabaseName("ix_identity_users_normalized_email");
            entity.HasIndex(user => user.NormalizedUserName).HasDatabaseName("ix_identity_users_normalized_user_name");
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("identity_roles");
            entity.Property(role => role.Id).HasColumnName("id");
            entity.Property(role => role.Name).HasColumnName("name");
            entity.Property(role => role.NormalizedName).HasColumnName("normalized_name");
            entity.Property(role => role.ConcurrencyStamp).HasColumnName("concurrency_stamp");
            entity.HasIndex(role => role.NormalizedName).HasDatabaseName("ix_identity_roles_normalized_name");
        });

        builder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("identity_user_roles");
            entity.Property(userRole => userRole.UserId).HasColumnName("user_id");
            entity.Property(userRole => userRole.RoleId).HasColumnName("role_id");
        });

        builder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("identity_user_claims");
            entity.Property(claim => claim.Id).HasColumnName("id");
            entity.Property(claim => claim.UserId).HasColumnName("user_id");
            entity.Property(claim => claim.ClaimType).HasColumnName("claim_type");
            entity.Property(claim => claim.ClaimValue).HasColumnName("claim_value");
        });

        builder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("identity_user_logins");
            entity.Property(login => login.LoginProvider).HasColumnName("login_provider");
            entity.Property(login => login.ProviderKey).HasColumnName("provider_key");
            entity.Property(login => login.ProviderDisplayName).HasColumnName("provider_display_name");
            entity.Property(login => login.UserId).HasColumnName("user_id");
        });

        builder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("identity_role_claims");
            entity.Property(claim => claim.Id).HasColumnName("id");
            entity.Property(claim => claim.RoleId).HasColumnName("role_id");
            entity.Property(claim => claim.ClaimType).HasColumnName("claim_type");
            entity.Property(claim => claim.ClaimValue).HasColumnName("claim_value");
        });

        builder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("identity_user_tokens");
            entity.Property(token => token.UserId).HasColumnName("user_id");
            entity.Property(token => token.LoginProvider).HasColumnName("login_provider");
            entity.Property(token => token.Name).HasColumnName("name");
            entity.Property(token => token.Value).HasColumnName("value");
        });
    }
}
