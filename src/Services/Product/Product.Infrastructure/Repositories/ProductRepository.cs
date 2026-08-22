using Microsoft.EntityFrameworkCore;
using Product.Application.Abstractions;
using Product.Infrastructure.Persistence;

namespace Product.Infrastructure.Repositories;

public class ProductRepository(ProductDbContext db) : IProductRepository
{
    public Task<Domain.Product?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(IReadOnlyCollection<Domain.Product> Items, int TotalCount)> SearchAsync(
        string? search, Guid? categoryId, bool? isActive, Domain.ProductType? productType, int page, int pageSize, CancellationToken ct)
    {
        var q = db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            q = q.Where(p => EF.Functions.Like(p.Name, $"%{term}%") || EF.Functions.Like(p.SKU, $"%{term}%"));
        }

        if (categoryId is { } category)
            q = q.Where(p => p.CategoryId == category);

        if (isActive is { } active)
            q = q.Where(p => p.IsActive == active);

        if (productType is { } type)
            q = q.Where(p => p.ProductType == type);

        var total = await q.CountAsync(ct);

        var clampedPage = Math.Max(page, 1);
        var clampedPageSize = Math.Clamp(pageSize, 1, 100);

        var items = await q
            .OrderBy(p => p.Name)
            .Skip((clampedPage - 1) * clampedPageSize)
            .Take(clampedPageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task AddAsync(Domain.Product product, CancellationToken ct) =>
        await db.Products.AddAsync(product, ct);

    public Task<bool> SkuExistsAsync(string sku, Guid? excludeId, CancellationToken ct) =>
        db.Products.AnyAsync(p => p.SKU == sku && (excludeId == null || p.Id != excludeId), ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
