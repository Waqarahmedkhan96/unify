using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Unify.Erp.Contracts.Auth;
using Unify.Erp.Domain.Accounting;
using Unify.Erp.Domain.Branches;
using Unify.Erp.Domain.Customers;
using Unify.Erp.Domain.Inventory;
using Unify.Erp.Domain.Organisations;
using Unify.Erp.Domain.Products;
using Unify.Erp.Domain.Suppliers;
using Unify.Erp.Domain.Warehouses;
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

        await EnsureOperatingDataAsync(dbContext, cancellationToken);

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

    private static async Task EnsureOperatingDataAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
    {
        const string organisationName = "Main Organisation";
        var organisation = await dbContext.Organisations
            .SingleOrDefaultAsync(item => item.DisplayName == organisationName, cancellationToken);

        if (organisation is null)
        {
            organisation = new Organisation(Guid.NewGuid(), "Unify Demo Trading LLC", organisationName, "PKR", "Asia/Karachi");
            dbContext.Organisations.Add(organisation);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var branch = await dbContext.Branches
            .SingleOrDefaultAsync(item => item.OrganisationId == organisation.Id && item.Code == "MAIN", cancellationToken);
        if (branch is null)
        {
            branch = new Branch(Guid.NewGuid(), organisation.Id, "MAIN", "Main Branch", "Asia/Karachi");
            dbContext.Branches.Add(branch);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var warehouse = await dbContext.Warehouses
            .SingleOrDefaultAsync(item => item.OrganisationId == organisation.Id && item.Code == "MAIN", cancellationToken);
        if (warehouse is null)
        {
            warehouse = new Warehouse(Guid.NewGuid(), organisation.Id, branch.Id, "MAIN", "Main Warehouse");
            dbContext.Warehouses.Add(warehouse);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var unit = await dbContext.UnitsOfMeasure
            .SingleOrDefaultAsync(item => item.OrganisationId == organisation.Id && item.Code == "PCS", cancellationToken);
        if (unit is null)
        {
            unit = new UnitOfMeasure(Guid.NewGuid(), organisation.Id, "PCS", "Pieces", 0);
            dbContext.UnitsOfMeasure.Add(unit);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var category = await dbContext.ProductCategories
            .SingleOrDefaultAsync(item => item.OrganisationId == organisation.Id && item.Code == "LPG", cancellationToken);
        if (category is null)
        {
            category = new ProductCategory(Guid.NewGuid(), organisation.Id, "LPG", "LPG Operations");
            dbContext.ProductCategories.Add(category);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var products = new[]
        {
            ("CYL-12", "Cylinder 12kg", 5500m, 6800m),
            ("CYL-45", "Cylinder 45kg", 15500m, 18800m),
            ("REG-12", "Regulator 12kg", 1200m, 1800m)
        };

        foreach (var seedProduct in products)
        {
            var product = await dbContext.Products
                .SingleOrDefaultAsync(item => item.OrganisationId == organisation.Id && item.ProductCode == seedProduct.Item1, cancellationToken);
            if (product is null)
            {
                product = new Product(
                    Guid.NewGuid(),
                    organisation.Id,
                    unit.Id,
                    category.Id,
                    seedProduct.Item1,
                    seedProduct.Item2,
                    null,
                    seedProduct.Item3,
                    seedProduct.Item4,
                    true);
                dbContext.Products.Add(product);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var balance = await dbContext.StockBalances.SingleOrDefaultAsync(
                item => item.OrganisationId == organisation.Id
                    && item.WarehouseId == warehouse.Id
                    && item.ProductId == product.Id,
                cancellationToken);
            if (balance is null)
            {
                balance = new StockBalance(Guid.NewGuid(), organisation.Id, warehouse.Id, product.Id);
                balance.Apply(250);
                dbContext.StockBalances.Add(balance);
                dbContext.StockMovements.Add(new StockMovement(
                    Guid.NewGuid(),
                    organisation.Id,
                    branch.Id,
                    warehouse.Id,
                    product.Id,
                    StockMovementType.AdjustmentIn,
                    250,
                    "DevelopmentSeed",
                    null,
                    "Initial demo stock",
                    DateTimeOffset.UtcNow));
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        var customers = new[]
        {
            ("C-1001", "North Retail", "0300-1001001", "north@example.com", 250000m),
            ("C-1002", "Al Noor Market", "0300-1001002", "alnoor@example.com", 150000m),
            ("C-1003", "City Wholesale", "0300-1001003", "city@example.com", 800000m)
        };

        foreach (var seedCustomer in customers)
        {
            var exists = await dbContext.Customers.AnyAsync(
                item => item.OrganisationId == organisation.Id && item.CustomerNumber == seedCustomer.Item1,
                cancellationToken);
            if (!exists)
            {
                dbContext.Customers.Add(new Customer(
                    Guid.NewGuid(),
                    organisation.Id,
                    branch.Id,
                    seedCustomer.Item1,
                    seedCustomer.Item2,
                    seedCustomer.Item2,
                    seedCustomer.Item3,
                    seedCustomer.Item4,
                    null,
                    seedCustomer.Item5));
            }
        }

        var suppliers = new[]
        {
            ("S-1001", "Gas Supply Co", "0300-2002001", "supplier@example.com"),
            ("S-1002", "Cylinder Works", "0300-2002002", "cylinder@example.com")
        };

        foreach (var seedSupplier in suppliers)
        {
            var exists = await dbContext.Suppliers.AnyAsync(
                item => item.OrganisationId == organisation.Id && item.SupplierNumber == seedSupplier.Item1,
                cancellationToken);
            if (!exists)
            {
                dbContext.Suppliers.Add(new Supplier(
                    Guid.NewGuid(),
                    organisation.Id,
                    seedSupplier.Item1,
                    seedSupplier.Item2,
                    seedSupplier.Item2,
                    seedSupplier.Item3,
                    seedSupplier.Item4,
                    null));
            }
        }

        var accounts = new[]
        {
            ("1000", "Cash", AccountType.Asset),
            ("1100", "Accounts Receivable", AccountType.Asset),
            ("1200", "Inventory", AccountType.Asset),
            ("2000", "Accounts Payable", AccountType.Liability),
            ("3000", "Owner Equity", AccountType.Equity),
            ("4000", "Sales Revenue", AccountType.Revenue),
            ("5000", "Cost of Goods Sold", AccountType.Expense)
        };

        foreach (var seedAccount in accounts)
        {
            var exists = await dbContext.Accounts.AnyAsync(
                item => item.OrganisationId == organisation.Id && item.Code == seedAccount.Item1,
                cancellationToken);
            if (!exists)
            {
                dbContext.Accounts.Add(new Account(
                    Guid.NewGuid(),
                    organisation.Id,
                    seedAccount.Item1,
                    seedAccount.Item2,
                    seedAccount.Item3));
            }
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fiscalYearStart = new DateOnly(today.Year, 1, 1);
        var fiscalYearEnd = new DateOnly(today.Year, 12, 31);
        var hasCurrentPeriod = await dbContext.FiscalPeriods.AnyAsync(
            item => item.OrganisationId == organisation.Id
                && item.StartsOn <= today
                && item.EndsOn >= today,
            cancellationToken);
        if (!hasCurrentPeriod)
        {
            dbContext.FiscalPeriods.Add(new FiscalPeriod(
                Guid.NewGuid(),
                organisation.Id,
                $"FY {today.Year}",
                fiscalYearStart,
                fiscalYearEnd));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
