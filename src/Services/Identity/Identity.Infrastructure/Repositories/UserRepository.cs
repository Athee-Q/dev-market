using Identity.Application.Abstractions;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class UserRepository(IdentityDbContext db) : IUserRepository
{
    public Task<Domain.User?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<Domain.User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var normalized = Domain.User.Normalize(email);
        return db.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct)
    {
        var normalized = Domain.User.Normalize(email);
        return db.Users.AnyAsync(u => u.NormalizedEmail == normalized, ct);
    }

    public async Task<(IReadOnlyCollection<Domain.User> Items, int TotalCount)> SearchAsync(int page, int pageSize, CancellationToken ct)
    {
        var query = db.Users.Include(u => u.UserRoles).OrderBy(u => u.CreatedAt);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(Domain.User user, CancellationToken ct) => await db.Users.AddAsync(user, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
