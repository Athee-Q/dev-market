using System.Security.Claims;
using ECommerce.Contracts.Common;

namespace ECommerce.BuildingBlocks.Auth;

/// <summary>Reads the claims Identity Service puts on every access token.</summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>The authenticated user's id (also the CustomerId — see UserRegisteredEvent). Throws if the caller isn't authenticated.</summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new ValidationAppException("Request is missing an authenticated user id.");
        return Guid.Parse(value);
    }

    public static string? GetEmail(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Email);

    public static string? GetFullName(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Name);

    public static bool HasPermission(this ClaimsPrincipal user, string permission) =>
        user.HasClaim(AppClaimTypes.Permission, permission);
}
