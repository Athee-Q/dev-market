namespace Identity.Domain;

/// <summary>Join row: one user granted one role. Composite key (UserId, RoleId) — see IdentityDbContext.</summary>
public class UserRole
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public Role? Role { get; private set; }

    private UserRole() { }

    public UserRole(Guid userId, Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }
}
