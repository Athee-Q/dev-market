using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<Domain.User> Users => Set<Domain.User>();
    public DbSet<Domain.Role> Roles => Set<Domain.Role>();
    public DbSet<Domain.Permission> Permissions => Set<Domain.Permission>();
    public DbSet<Domain.UserRole> UserRoles => Set<Domain.UserRole>();
    public DbSet<Domain.RolePermission> RolePermissions => Set<Domain.RolePermission>();
    public DbSet<Domain.RefreshToken> RefreshTokens => Set<Domain.RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}
