using ECommerce.BuildingBlocks.Application.Behaviors;
using ECommerce.BuildingBlocks.Application.Mediator;
using Product.Application.Abstractions;
using Product.Application.Dto;

namespace Product.Application.Features.GetProductById;

/// <summary>
/// Single product lookup — the hottest read path in the service, so it opts into
/// HybridCachingBehaviour's two-tier cache: a 30s in-memory L1 in front of the 10min Redis L2.
/// Invalidated explicitly by UpdateProduct via IProductCacheInvalidator (evicts both tiers, on
/// every instance — see that interface's doc comment). See IHybridCacheableQuery.
/// </summary>
public static class GetProductById
{
    public record Query(Guid Id) : IRequest<ProductDto?>, IHybridCacheableQuery
    {
        public string CacheKey => $"product:{Id}";
        public TimeSpan Expiration => TimeSpan.FromMinutes(10);
        public TimeSpan LocalExpiration => TimeSpan.FromSeconds(30);
    }

    public class Handler(IProductRepository repository) : IRequestHandler<Query, ProductDto?>
    {
        public async Task<ProductDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var product = await repository.GetByIdAsync(request.Id, cancellationToken);
            return product is null ? null : ProductDto.FromDomain(product);
        }
    }
}
