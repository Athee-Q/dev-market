namespace Identity.Application.Abstractions;

public interface IUserRepository
{
    Task<Domain.User?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Looks up by <see cref="Domain.User.NormalizedEmail"/> — callers pass the raw email, the repository normalizes it.</summary>
    Task<Domain.User?> GetByEmailAsync(string email, CancellationToken ct);

    Task<bool> EmailExistsAsync(string email, CancellationToken ct);

    Task<(IReadOnlyCollection<Domain.User> Items, int TotalCount)> SearchAsync(int page, int pageSize, CancellationToken ct);

    Task AddAsync(Domain.User user, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public interface IRoleRepository
{
    Task<Domain.Role?> GetByNameAsync(string name, CancellationToken ct);
    Task<IReadOnlyCollection<Domain.Role>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
    Task<IReadOnlyCollection<Domain.Role>> GetAllAsync(CancellationToken ct);
}

public interface IRefreshTokenRepository
{
    Task<Domain.RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct);
    Task AddAsync(Domain.RefreshToken token, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

/// <summary>Hashes/verifies passwords — implemented in Infrastructure via ASP.NET Core's PasswordHasher&lt;User&gt;.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

/// <summary>
/// Issues the JWTs and refresh tokens Identity Service hands out. Implemented in Infrastructure
/// (reads Jwt:* config there) so Application stays free of a JWT library dependency.
/// </summary>
public interface ITokenService
{
    (string AccessToken, DateTimeOffset ExpiresAt) CreateAccessToken(
        Guid userId, string email, string fullName, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions);

    (string RawToken, string TokenHash, DateTimeOffset ExpiresAt) CreateRefreshToken();

    string HashToken(string rawToken);
}

/// <summary>Abstraction over the message bus so Application stays free of a MassTransit dependency.</summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken ct) where T : class;
}
