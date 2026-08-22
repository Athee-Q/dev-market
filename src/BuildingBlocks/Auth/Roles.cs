namespace ECommerce.BuildingBlocks.Auth;

/// <summary>
/// Well-known role names, shared between Identity Service (which owns the Roles table and seeds
/// these) and every other service (which only ever checks role/permission claims off the JWT —
/// nobody but Identity Service queries the Roles table directly).
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
}
