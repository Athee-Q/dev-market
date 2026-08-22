namespace Product.Application.Abstractions;

public interface IProductRepository
{
    Task<Domain.Product?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<(IReadOnlyCollection<Domain.Product> Items, int TotalCount)> SearchAsync(
        string? search, Guid? categoryId, bool? isActive, Domain.ProductType? productType, int page, int pageSize, CancellationToken ct);

    Task AddAsync(Domain.Product product, CancellationToken ct);
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
