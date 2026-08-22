namespace Identity.Domain;

/// <summary>A named permission (e.g. "products:manage" — see ECommerce.BuildingBlocks.Auth.Permissions), granted to roles via RolePermission.</summary>
public class Permission
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    private Permission() { }

    public Permission(string name, string? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }
}
