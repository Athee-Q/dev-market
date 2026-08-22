namespace ECommerce.BuildingBlocks.Auth;

/// <summary>
/// Every permission any service in the solution checks for, in one place. Identity Service seeds
/// these into the Permissions table and maps them to roles (RolePermission); every other service
/// only ever sees them as "permission" claims already baked into the JWT (see AppClaimTypes) and
/// registers an authorization policy per permission via AddJwtAuthentication — nobody but Identity
/// Service needs a database round-trip to answer "can this user do X".
/// </summary>
public static class Permissions
{
    // Product catalog
    public const string ProductsManage = "products:manage";

    // Orders
    public const string OrdersManage = "orders:manage"; // search/view any customer's orders

    // Customers
    public const string CustomersManage = "customers:manage";

    // Payments
    public const string PaymentsManage = "payments:manage";

    // Identity — user/role administration
    public const string UsersManage = "users:manage";

    /// <summary>Every permission that exists — used to seed the Permissions table and to register one authorization policy per permission.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        ProductsManage,
        OrdersManage,
        CustomersManage,
        PaymentsManage,
        UsersManage,
    ];

    /// <summary>Permissions granted to each seeded role. Admin gets everything; Customer gets none (self-service endpoints only need authentication, not a specific permission).</summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> ByRole = new Dictionary<string, IReadOnlyList<string>>
    {
        [Roles.Admin] = All,
        [Roles.Customer] = [],
    };
}
