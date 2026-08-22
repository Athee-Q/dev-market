namespace ECommerce.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Opts a query into HybridCachingBehaviour: a two-tier cache-aside, local in-memory (L1) in front
/// of the same Redis-backed distributed cache (L2) that plain ICacheableQuery uses. Deliberately a
/// separate interface rather than extending ICacheableQuery, so CachingBehaviour and
/// HybridCachingBehaviour never both act on the same request.
/// </summary>
public interface IHybridCacheableQuery
{
    string CacheKey { get; }

    /// <summary>L2 (Redis) time-to-live — same role as ICacheableQuery.Expiration.</summary>
    TimeSpan Expiration { get; }

    /// <summary>
    /// L1 (in-memory, per-instance) time-to-live — shorter than Expiration. Bounds how stale a
    /// local cache entry can get on its own; RedisProductCacheInvalidator's pub/sub notification
    /// (see Product.Infrastructure) closes that window on writes instead of waiting it out.
    /// </summary>
    TimeSpan LocalExpiration { get; }
}
