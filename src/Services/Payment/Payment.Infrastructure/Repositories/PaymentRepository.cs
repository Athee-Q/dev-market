using Microsoft.EntityFrameworkCore;
using Payment.Application.Abstractions;
using Payment.Domain;
using Payment.Infrastructure.Persistence;

namespace Payment.Infrastructure.Repositories;

public class PaymentRepository(PaymentDbContext db) : IPaymentRepository
{
    public Task<Domain.Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct) =>
        db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId, ct);

    public Task<Domain.Payment?> GetByRazorpayOrderIdAsync(string razorpayOrderId, CancellationToken ct) =>
        db.Payments.FirstOrDefaultAsync(p => p.RazorpayOrderId == razorpayOrderId, ct);

    public async Task<(IReadOnlyCollection<Domain.Payment> Items, int TotalCount)> SearchAsync(
        Guid? customerId, PaymentStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var q = db.Payments.AsNoTracking().AsQueryable();

        if (customerId is { } customer)
            q = q.Where(p => p.CustomerId == customer);

        if (status is { } s)
            q = q.Where(p => p.Status == s);

        var total = await q.CountAsync(ct);

        var clampedPage = Math.Max(page, 1);
        var clampedPageSize = Math.Clamp(pageSize, 1, 100);

        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((clampedPage - 1) * clampedPageSize)
            .Take(clampedPageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<(decimal TotalRevenue, int SucceededCount)> GetRevenueSummaryAsync(CancellationToken ct)
    {
        var succeeded = db.Payments.AsNoTracking().Where(p => p.Status == PaymentStatus.Succeeded);
        var total = await succeeded.SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;
        var count = await succeeded.CountAsync(ct);
        return (total, count);
    }

    public async Task AddAsync(Domain.Payment payment, CancellationToken ct) =>
        await db.Payments.AddAsync(payment, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
