namespace Identity.Domain;

/// <summary>Join row: one role granted one permission. Composite key (RoleId, PermissionId) — see IdentityDbContext.</summary>
public class RolePermission
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public Permission? Permission { get; private set; }

    private RolePermission() { }

    public RolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }
}
