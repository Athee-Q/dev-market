namespace Identity.Domain;

/// <summary>
/// An issued refresh token, stored as a SHA-256 hash (never the raw token — same principle as a
/// password hash) so a DB leak doesn't hand out live sessions. Rotated on every use: Login/Refresh
/// issues a new row and revokes the old one, chaining ReplacedByTokenHash so reuse of an already-
/// rotated token can be detected (not currently acted on beyond rejection — see README Known Gaps).
/// </summary>
public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = default!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;

    private RefreshToken() { }

    public RefreshToken(Guid userId, string tokenHash, DateTimeOffset expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke(string? replacedByTokenHash = null)
    {
        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
