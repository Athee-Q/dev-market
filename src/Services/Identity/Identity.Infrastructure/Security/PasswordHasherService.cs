using Identity.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace Identity.Infrastructure.Security;

/// <summary>
/// Wraps ASP.NET Core's own PasswordHasher&lt;TUser&gt; (PBKDF2, part of the shared framework —
/// see the FrameworkReference in Identity.Infrastructure.csproj) rather than adding a third-party
/// hashing package. The generic parameter is unused by the default implementation, so `null!` is
/// safe here — the hash is derived only from the password and a random per-hash salt.
/// </summary>
public class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<Domain.User> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(null!, password);

    public bool Verify(string password, string hash) =>
        _hasher.VerifyHashedPassword(null!, hash, password) != PasswordVerificationResult.Failed;
}
