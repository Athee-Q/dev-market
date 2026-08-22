using Identity.Application.Abstractions;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class RoleRepository(IdentityDbContext db) : IRoleRepository
{
    public Task<Domain.Role?> GetByNameAsync(string name, CancellationToken ct) =>
        db.Roles.FirstOrDefaultAsync(r => r.Name == name, ct);

    public async Task<IReadOnlyCollection<Domain.Role>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var idList = ids.ToList();
        return await db.Roles.Where(r => idList.Contains(r.Id)).ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Domain.Role>> GetAllAsync(CancellationToken ct) =>
        await db.Roles.ToListAsync(ct);
}
