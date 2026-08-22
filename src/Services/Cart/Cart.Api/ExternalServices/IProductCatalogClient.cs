namespace Cart.Api.ExternalServices;

public record ProductInfo(Guid Id, string Name, decimal Price, bool IsActive);

public interface IProductCatalogClient
{
    Task<ProductInfo?> GetProductAsync(Guid productId, CancellationToken ct);
}
