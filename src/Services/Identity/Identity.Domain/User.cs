namespace Identity.Domain;

/// <summary>
/// One authentication account. Deliberately separate from Customer.Domain.Customer (business
/// profile — name/email/addresses used on orders): this is credentials + roles only. Id is the
/// same GUID as the Customer row created for it (see UserRegisteredEvent), so every other service
/// can keep treating "the authenticated user" and "the customer" as one id.
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = default!;
    public string NormalizedEmail { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private User() { }

    public User(string email, string fullName, string passwordHash)
    {
        Id = Guid.NewGuid();
        Email = email;
        NormalizedEmail = Normalize(email);
        FullName = fullName;
        PasswordHash = passwordHash;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public static string Normalize(string email) => email.Trim().ToUpperInvariant();

    public void AssignRole(Guid roleId)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId)) return;
        _userRoles.Add(new UserRole(Id, roleId));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
