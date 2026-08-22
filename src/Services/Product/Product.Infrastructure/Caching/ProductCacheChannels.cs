namespace Product.Infrastructure.Caching;

/// <summary>Redis pub/sub channel name(s) shared between RedisProductCacheInvalidator (publisher)
/// and ProductCacheInvalidationSubscriber (subscriber) — kept in one place so the two sides can't drift.</summary>
internal static class ProductCacheChannels
{
    public const string Invalidation = "product-cache:invalidate";
}
