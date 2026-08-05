using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Unify.Erp.Application.Auth;
using Unify.Erp.Application.Accounting;
using Unify.Erp.Application.Customers;
using Unify.Erp.Application.Inventory;
using Unify.Erp.Application.Payments;
using Unify.Erp.Application.Platform;
using Unify.Erp.Application.Products;
using Unify.Erp.Application.Purchasing;
using Unify.Erp.Application.Sales;
using Unify.Erp.Application.Suppliers;
using Unify.Erp.Infrastructure.Auth;
using Unify.Erp.Infrastructure.Accounting;
using Unify.Erp.Infrastructure.Customers;
using Unify.Erp.Infrastructure.Identity;
using Unify.Erp.Infrastructure.Inventory;
using Unify.Erp.Infrastructure.Persistence;
using Unify.Erp.Infrastructure.Platform;
using Unify.Erp.Infrastructure.Payments;
using Unify.Erp.Infrastructure.Products;
using Unify.Erp.Infrastructure.Purchasing;
using Unify.Erp.Infrastructure.Sales;
using Unify.Erp.Infrastructure.Seed;
using Unify.Erp.Infrastructure.Suppliers;

namespace Unify.Erp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<DevelopmentSeedOptions>(configuration.GetSection(DevelopmentSeedOptions.SectionName));
        services.AddScoped<JwtTokenFactory>();
        services.AddScoped<IAuthenticationService, Auth.AuthenticationService>();
        services.AddScoped<IOrganisationService, OrganisationService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IProductCatalogService, ProductCatalogService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ISalesService, SalesService>();
        services.AddScoped<ICustomerPaymentService, CustomerPaymentService>();
        services.AddScoped<IPurchasingService, PurchasingService>();
        services.AddScoped<IAccountingService, AccountingService>();

        var connectionString = configuration.GetConnectionString("Default");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        return services;
    }
}
