namespace Identity.Domain;

/// <summary>A named role (e.g. "Admin", "Customer" — see ECommerce.BuildingBlocks.Auth.Roles) granting zero or more permissions.</summary>
public class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role() { }

    public Role(string name, string? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }

    public void GrantPermission(Guid permissionId)
    {
        if (_rolePermissions.Any(rp => rp.PermissionId == permissionId)) return;
        _rolePermissions.Add(new RolePermission(Id, permissionId));
    }
}
