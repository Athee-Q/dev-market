using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.BuildingBlocks.Auth;

public static class JwtAuthenticationExtensions
{
    /// <summary>
    /// Adds JWT bearer authentication plus one authorization policy per permission in
    /// <see cref="Permissions.All"/> (e.g. "products:manage"). Every resource service calls this
    /// the same way — only Identity Service additionally *issues* tokens (see Identity.Infrastructure's
    /// JwtTokenService); everyone else, including Identity Service itself for its own /me endpoint,
    /// only ever validates them, statelessly, with no callback to Identity Service.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwt.SigningKey) || Encoding.UTF8.GetByteCount(jwt.SigningKey) < 32)
            throw new InvalidOperationException(
                "Configuration 'Jwt:SigningKey' is missing or shorter than 32 bytes (256 bits) — required for HS256. Set JWT_SIGNING_KEY in .env.");

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorizationBuilder();
        services.PostConfigure<Microsoft.AspNetCore.Authorization.AuthorizationOptions>(options =>
        {
            foreach (var permission in Permissions.All)
                options.AddPolicy(permission, policy => policy.RequireClaim(AppClaimTypes.Permission, permission));
        });

        return services;
    }
}
