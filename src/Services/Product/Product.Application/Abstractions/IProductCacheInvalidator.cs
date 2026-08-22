namespace Product.Application.Abstractions;

/// <summary>
/// Invalidates the cache entry for one product on both cache tiers: the shared Redis L2 (so the
/// next request on any instance re-fetches from the database) and — via Redis pub/sub — every
/// instance's local in-memory L1 (so a request already routed to another instance doesn't keep
/// serving a stale in-process value until LocalExpiration passes). One call so a future write path
/// can't invalidate L2 and forget to notify L1. Implemented by RedisProductCacheInvalidator
/// (Product.Infrastructure) — Application only owns the abstraction, same convention as IProductRepository.
/// </summary>
public interface IProductCacheInvalidator
{
    Task InvalidateAsync(Guid productId, CancellationToken cancellationToken);
}
