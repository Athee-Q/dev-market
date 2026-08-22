using ECommerce.BuildingBlocks.Auth;
using Identity.Application.Abstractions;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Identity.Infrastructure.Seed;

/// <summary>
/// Runs once at startup, right after the EF migration (same "auto-apply, no manual step" pattern
/// every other service uses) — seeds every Permission, the Admin/Customer roles with
/// Permissions.ByRole, and one bootstrap Admin user from config so there's a way to log in as
/// Admin on a fresh database. Every step is idempotent (checked-then-create) so it's safe to run
/// on every restart, not just the first one.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IdentityDbContext db, IConfiguration configuration, IPasswordHasher passwordHasher, CancellationToken ct)
    {
        var permissionByName = new Dictionary<string, Domain.Permission>();
        foreach (var name in Permissions.All)
        {
            var permission = await db.Permissions.FirstOrDefaultAsync(p => p.Name == name, ct);
            if (permission is null)
            {
                permission = new Domain.Permission(name);
                db.Permissions.Add(permission);
            }
            permissionByName[name] = permission;
        }
        await db.SaveChangesAsync(ct);

        var roleByName = new Dictionary<string, Domain.Role>();
        foreach (var roleName in Permissions.ByRole.Keys)
        {
            // Include RolePermissions so GrantPermission's dedupe check below sees what's already
            // granted — without it, an existing role's collection reads as empty and every
            // restart tries (and fails, on the composite key) to re-insert the same grants.
            var role = await db.Roles.Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.Name == roleName, ct);
            if (role is null)
            {
                role = new Domain.Role(roleName);
                db.Roles.Add(role);
            }
            roleByName[roleName] = role;
        }
        await db.SaveChangesAsync(ct);

        foreach (var (roleName, grantedPermissions) in Permissions.ByRole)
        {
            var role = roleByName[roleName];
            foreach (var permissionName in grantedPermissions)
                role.GrantPermission(permissionByName[permissionName].Id);
        }
        await db.SaveChangesAsync(ct);

        var adminEmail = configuration["Identity:SeedAdmin:Email"];
        var adminPassword = configuration["Identity:SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        var normalized = Domain.User.Normalize(adminEmail);
        if (await db.Users.AnyAsync(u => u.NormalizedEmail == normalized, ct))
            return;

        var admin = new Domain.User(adminEmail, "Admin", passwordHasher.Hash(adminPassword));
        admin.AssignRole(roleByName[Roles.Admin].Id);
        db.Users.Add(admin);
        await db.SaveChangesAsync(ct);
    }
}
