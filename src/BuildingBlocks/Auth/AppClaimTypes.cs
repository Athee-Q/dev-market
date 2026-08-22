namespace ECommerce.BuildingBlocks.Auth;

/// <summary>Custom JWT claim types Identity Service issues, on top of the standard sub/email/name/role ones.</summary>
public static class AppClaimTypes
{
    /// <summary>One claim per permission the user's role(s) grant — see <see cref="Permissions"/>.</summary>
    public const string Permission = "permission";
}
