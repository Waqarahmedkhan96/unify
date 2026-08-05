namespace Unify.Erp.Contracts.Auth;

public static class PermissionNames
{
    public const string ClaimType = "permission";

    public const string PlatformManage = "platform.manage";
    public const string CustomersManage = "customers.manage";
    public const string SuppliersManage = "suppliers.manage";
    public const string ProductsManage = "products.manage";
    public const string InventoryManage = "inventory.manage";
    public const string SalesManage = "sales.manage";
    public const string PaymentsManage = "payments.manage";
    public const string PurchasingManage = "purchasing.manage";
    public const string AccountingManage = "accounting.manage";

    public static readonly string[] All =
    [
        PlatformManage,
        CustomersManage,
        SuppliersManage,
        ProductsManage,
        InventoryManage,
        SalesManage,
        PaymentsManage,
        PurchasingManage,
        AccountingManage
    ];
}
