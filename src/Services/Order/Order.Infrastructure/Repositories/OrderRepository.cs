using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions;
using Order.Domain;
using Order.Infrastructure.Persistence;

namespace Order.Infrastructure.Repositories;

public class OrderRepository(OrderDbContext db) : IOrderRepository
{
    public Task<Domain.Order?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<(IReadOnlyCollection<Domain.Order> Items, int TotalCount)> SearchAsync(
        Guid? customerId, OrderStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var q = db.Orders.Include(o => o.Items).AsNoTracking().AsQueryable();

        if (customerId is { } customer)
            q = q.Where(o => o.CustomerId == customer);

        if (status is { } orderStatus)
            q = q.Where(o => o.Status == orderStatus);

        var total = await q.CountAsync(ct);

        var clampedPage = Math.Max(page, 1);
        var clampedPageSize = Math.Clamp(pageSize, 1, 100);

        var items = await q
            .OrderByDescending(o => o.CreatedAt)
            .Skip((clampedPage - 1) * clampedPageSize)
            .Take(clampedPageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(Domain.Order order, CancellationToken ct) =>
        await db.Orders.AddAsync(order, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
