using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Product.Infrastructure.Caching;

/// <summary>
/// Runs once per Product Service instance. Subscribes to ProductCacheChannels.Invalidation and, for
/// every product id another instance (or this one) publishes there, evicts that product's entry
/// from this instance's local in-memory (L1) cache — closing the cross-instance staleness window
/// that a per-process L1 cache would otherwise leave open for up to GetProductById's LocalExpiration.
/// The shared Redis (L2) cache needs no such fan-out: every instance already reads/writes the same
/// Redis, so RedisProductCacheInvalidator's plain RemoveAsync is enough for that tier.
/// </summary>
public class ProductCacheInvalidationSubscriber(
    IConnectionMultiplexer redis, IMemoryCache localCache, ILogger<ProductCacheInvalidationSubscriber> logger)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var subscriber = redis.GetSubscriber();
        await subscriber.SubscribeAsync(RedisChannel.Literal(ProductCacheChannels.Invalidation), (_, message) =>
        {
            if (!Guid.TryParse((string?)message, out var productId))
            {
                logger.LogWarning("Ignoring malformed product cache invalidation message: {Message}", message);
                return;
            }

            localCache.Remove($"product:{productId}");
        });
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
