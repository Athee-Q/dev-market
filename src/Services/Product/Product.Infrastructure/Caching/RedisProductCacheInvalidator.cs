using Microsoft.Extensions.Caching.Distributed;
using Product.Application.Abstractions;
using StackExchange.Redis;

namespace Product.Infrastructure.Caching;

/// <summary>See IProductCacheInvalidator. Removes the Redis (L2) entry, then publishes the product
/// id on ProductCacheChannels.Invalidation so ProductCacheInvalidationSubscriber can evict the
/// matching entry from every instance's local (L1) IMemoryCache too.</summary>
public class RedisProductCacheInvalidator(IDistributedCache cache, IConnectionMultiplexer redis) : IProductCacheInvalidator
{
    public async Task InvalidateAsync(Guid productId, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync($"product:{productId}", cancellationToken);

        var subscriber = redis.GetSubscriber();
        await subscriber.PublishAsync(RedisChannel.Literal(ProductCacheChannels.Invalidation), productId.ToString());
    }
}
