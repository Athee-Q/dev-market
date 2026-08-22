namespace ECommerce.BuildingBlocks.Auth;

/// <summary>
/// Bound from the "Jwt" configuration section — identical values must be configured on Identity
/// Service (which signs tokens) and every other service (which only validates them), so they all
/// read the same env-var-backed config keys (see docker-compose.yml's Jwt__* variables).
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "ECommerce.Identity";
    public string Audience { get; set; } = "ECommerce";
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;
}
