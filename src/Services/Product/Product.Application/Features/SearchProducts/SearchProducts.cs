using ECommerce.BuildingBlocks.Application.Behaviors;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using Product.Application.Abstractions;
using Product.Application.Dto;
using Product.Domain;

namespace Product.Application.Features.SearchProducts;

/// <summary>List/search products, page/pageSize (§ ProductsController.Search). Cached 30s — see ICacheableQuery.</summary>
public static class SearchProducts
{
    public record Query(string? Search, Guid? CategoryId, bool? IsActive, ProductType? ProductType, int Page = 1, int PageSize = 20)
        : IRequest<PagedResult<ProductDto>>, ICacheableQuery
    {
        // A short TTL bounds staleness instead of trying to invalidate every possible
        // search/page combination a write could affect — see CreateProduct/UpdateProduct.
        public string CacheKey => $"product:search:{Search}:{CategoryId}:{IsActive}:{ProductType}:{Page}:{PageSize}";
        public TimeSpan Expiration => TimeSpan.FromSeconds(30);
    }

    public class Handler(IProductRepository repository) : IRequestHandler<Query, PagedResult<ProductDto>>
    {
        public async Task<PagedResult<ProductDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var (items, total) = await repository.SearchAsync(
                request.Search, request.CategoryId, request.IsActive, request.ProductType, request.Page, request.PageSize, cancellationToken);

            return new PagedResult<ProductDto>(items.Select(ProductDto.FromDomain).ToList(), request.Page, request.PageSize, total);
        }
    }
}
