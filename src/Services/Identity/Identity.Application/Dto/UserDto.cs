namespace Identity.Application.Dto;

public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    bool IsActive,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<string> Roles)
{
    public static UserDto FromDomain(Domain.User user, IReadOnlyCollection<string> roleNames) =>
        new(user.Id, user.Email, user.FullName, user.IsActive, user.CreatedAt, roleNames);
}

/// <summary>Returned by Register/Login/RefreshToken — the access token plus the raw refresh token (never persisted raw server-side).</summary>
public record AuthResultDto(
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken);
