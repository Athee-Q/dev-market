using System.Text.Json;
using ECommerce.BuildingBlocks.Application.Mediator;
using Microsoft.Extensions.Caching.Distributed;

namespace ECommerce.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Redis-backed cache-aside behavior for any request that implements <see cref="ICacheableQuery"/>
/// — requests that don't are passed straight through untouched. Replaces the earlier hand-rolled
/// CachedProductService decorator: same cache-aside idea, now expressed once as a pipeline
/// behavior instead of a per-service decorator around a monolithic service interface.
/// </summary>
public class CachingBehaviour<TRequest, TResponse>(IDistributedCache cache) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheable)
            return await next();

        var cached = await cache.GetStringAsync(cacheable.CacheKey, cancellationToken);
        if (cached is not null)
            return JsonSerializer.Deserialize<TResponse>(cached)!;

        var response = await next();

        await cache.SetStringAsync(
            cacheable.CacheKey,
            JsonSerializer.Serialize(response),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheable.Expiration },
            cancellationToken);

        return response;
    }
}
