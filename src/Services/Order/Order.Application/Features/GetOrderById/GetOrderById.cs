using System.Text.Json;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using Microsoft.Extensions.Caching.Distributed;
using Order.Application.Abstractions;
using Order.Application.Dto;

namespace Order.Application.Features.GetOrderById;

public static class GetOrderById
{
    /// <summary>RequestingCustomerId/IsAdmin come from the caller's JWT (see GetOrderByIdEndpoint) — a customer can only fetch their own order, an Admin (OrdersManage) can fetch anyone's.</summary>
    public record Query(Guid Id, Guid RequestingCustomerId, bool IsAdmin) : IRequest<OrderDto?>;

    /// <summary>
    /// Deliberately NOT an ICacheableQuery — CachingBehaviour returns a cache hit before the handler
    /// ever runs, which would skip the ownership check below and let a cache hit leak another
    /// customer's order. Instead this is an explicit read-through: cache the order by id alone
    /// (RequestingCustomerId/IsAdmin are never part of the cached value or the key), but always
    /// re-run authorization against the resolved DTO — cached or not — before returning it.
    /// </summary>
    public class Handler(IOrderRepository repository, IDistributedCache cache) : IRequestHandler<Query, OrderDto?>
    {
        private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(5);

        public async Task<OrderDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var order = await GetOrderAsync(request.Id, cancellationToken);
            if (order is null) return null;

            if (!request.IsAdmin && order.CustomerId != request.RequestingCustomerId)
                throw new ForbiddenAppException("This order does not belong to you.");

            return order;
        }

        private async Task<OrderDto?> GetOrderAsync(Guid id, CancellationToken cancellationToken)
        {
            var cacheKey = OrderCacheKeys.Id(id);

            var cached = await cache.GetStringAsync(cacheKey, cancellationToken);
            if (cached is not null)
                return JsonSerializer.Deserialize<OrderDto>(cached);

            var order = await repository.GetByIdAsync(id, cancellationToken);
            if (order is null) return null;

            var dto = OrderDto.FromDomain(order);
            await cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(dto),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Expiration },
                cancellationToken);

            return dto;
        }
    }
}
