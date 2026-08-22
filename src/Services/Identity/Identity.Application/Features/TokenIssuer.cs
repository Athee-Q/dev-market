using ECommerce.BuildingBlocks.Auth;
using Identity.Application.Abstractions;
using Identity.Application.Dto;

namespace Identity.Application.Features;

/// <summary>
/// Shared by Register/Login/RefreshToken: turns a User + their Roles into a fresh access token
/// (with one permission claim per Permissions.ByRole entry the role grants — see AppClaimTypes)
/// plus a brand-new refresh token row. Callers that are rotating an existing refresh token are
/// responsible for revoking the old one themselves after calling this.
/// </summary>
internal static class TokenIssuer
{
    public static async Task<AuthResultDto> IssueAsync(
        Domain.User user,
        IReadOnlyCollection<Domain.Role> roles,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokens,
        CancellationToken ct)
    {
        var roleNames = roles.Select(r => r.Name).ToList();
        var permissions = roles
            .SelectMany(r => Permissions.ByRole.TryGetValue(r.Name, out var granted) ? granted : [])
            .Distinct()
            .ToList();

        var (accessToken, accessExpiresAt) = tokenService.CreateAccessToken(user.Id, user.Email, user.FullName, roleNames, permissions);
        var (rawRefresh, refreshHash, refreshExpiresAt) = tokenService.CreateRefreshToken();

        await refreshTokens.AddAsync(new Domain.RefreshToken(user.Id, refreshHash, refreshExpiresAt), ct);
        await refreshTokens.SaveChangesAsync(ct);

        return new AuthResultDto(user.Id, user.Email, user.FullName, roleNames, permissions, accessToken, accessExpiresAt, rawRefresh);
    }
}
