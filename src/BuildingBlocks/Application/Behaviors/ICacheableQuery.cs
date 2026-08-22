namespace ECommerce.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Opts a query into CachingBehaviour, cache-aside style. A request implements this structurally
/// — CachingBehaviour never needs to know about specific request types, only this shape.
/// </summary>
public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan Expiration { get; }
}
