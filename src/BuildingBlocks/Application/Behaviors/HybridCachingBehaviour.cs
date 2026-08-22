using System.Text.Json;
using ECommerce.BuildingBlocks.Application.Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace ECommerce.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Two-tier cache-aside behavior for any request implementing <see cref="IHybridCacheableQuery"/> —
/// requests that don't are passed straight through untouched, same convention as CachingBehaviour.
/// Checks the local in-memory cache (L1) first (no network round trip), then the distributed Redis
/// cache (L2, populating L1 on the way back out), and finally the handler itself (populating both).
/// L1 is per-instance, so a write on one instance invalidating L2 alone would leave every other
/// instance's L1 serving a stale value until LocalExpiration passes — see
/// Product.Infrastructure's RedisProductCacheInvalidator + ProductCacheInvalidationSubscriber for
/// how a write also evicts L1 on every instance via Redis pub/sub.
/// </summary>
public class HybridCachingBehaviour<TRequest, TResponse>(IMemoryCache local, IDistributedCache distributed)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IHybridCacheableQuery cacheable)
            return await next();

        if (local.TryGetValue(cacheable.CacheKey, out TResponse? localHit))
            return localHit!;

        var distributedHit = await distributed.GetStringAsync(cacheable.CacheKey, cancellationToken);
        if (distributedHit is not null)
        {
            var value = JsonSerializer.Deserialize<TResponse>(distributedHit)!;
            local.Set(cacheable.CacheKey, value, cacheable.LocalExpiration);
            return value;
        }

        var response = await next();

        await distributed.SetStringAsync(
            cacheable.CacheKey,
            JsonSerializer.Serialize(response),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheable.Expiration },
            cancellationToken);
        local.Set(cacheable.CacheKey, response, cacheable.LocalExpiration);

        return response;
    }
}
