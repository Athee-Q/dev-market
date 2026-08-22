using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Cart.Api.ExternalServices;

/// <summary>
/// Cache-aside decorator around ProductCatalogClient's outbound HTTP lookup — AddItemAsync calls
/// GetProductAsync on every add-to-cart, so without this every add re-hits the Product service over
/// HTTP even though Product.Api caches the same lookup on its own side. 60s TTL, short because
/// price/name can change and there's no invalidation path from Product's writes into this key.
/// </summary>
public class CachedProductCatalogClient(GrpcProductCatalogClient inner, IDistributedCache cache) : IProductCatalogClient
{
    private static readonly TimeSpan Expiration = TimeSpan.FromSeconds(60);

    public async Task<ProductInfo?> GetProductAsync(Guid productId, CancellationToken ct)
    {
        var cacheKey = $"product-lookup:{productId}";

        var cached = await cache.GetStringAsync(cacheKey, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<ProductInfo>(cached);

        var product = await inner.GetProductAsync(productId, ct);
        if (product is null) return null; // don't cache misses — a not-yet-existing product may appear moments later

        await cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(product),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Expiration },
            ct);

        return product;
    }
}
