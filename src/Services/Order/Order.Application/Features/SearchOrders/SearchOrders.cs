using ECommerce.BuildingBlocks.Application.Behaviors;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using Order.Application.Abstractions;
using Order.Application.Dto;
using Order.Domain;

namespace Order.Application.Features.SearchOrders;

/// <summary>List/search orders, page/pageSize. Cached 30s — see ICacheableQuery. A short TTL bounds
/// staleness instead of trying to invalidate every possible search/page combination a write could
/// affect, same rationale as Product's SearchProducts.</summary>
public static class SearchOrders
{
    public record Query(Guid? CustomerId, OrderStatus? Status, int Page = 1, int PageSize = 20)
        : IRequest<PagedResult<OrderDto>>, ICacheableQuery
    {
        public string CacheKey => $"order:search:{CustomerId}:{Status}:{Page}:{PageSize}";
        public TimeSpan Expiration => TimeSpan.FromSeconds(30);
    }

    public class Handler(IOrderRepository repository) : IRequestHandler<Query, PagedResult<OrderDto>>
    {
        public async Task<PagedResult<OrderDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var (items, total) = await repository.SearchAsync(
                request.CustomerId, request.Status, request.Page, request.PageSize, cancellationToken);

            return new PagedResult<OrderDto>(items.Select(OrderDto.FromDomain).ToList(), request.Page, request.PageSize, total);
        }
    }
}
