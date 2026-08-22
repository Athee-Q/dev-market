using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using ECommerce.Contracts.Events;
using Microsoft.Extensions.Caching.Distributed;
using Order.Application.Abstractions;
using Order.Application.Dto;

namespace Order.Application.Features.CancelOrder;

public static class CancelOrder
{
    /// <summary>RequestingCustomerId/IsAdmin come from the caller's JWT (see CancelOrderEndpoint).</summary>
    public record Command(Guid Id, Guid RequestingCustomerId, bool IsAdmin) : IRequest<OrderDto?>;

    public class Handler(IOrderRepository repository, IEventPublisher eventPublisher, IDistributedCache cache)
        : IRequestHandler<Command, OrderDto?>
    {
        public async Task<OrderDto?> Handle(Command request, CancellationToken cancellationToken)
        {
            var order = await repository.GetByIdAsync(request.Id, cancellationToken);
            if (order is null) return null;

            if (!request.IsAdmin && order.CustomerId != request.RequestingCustomerId)
                throw new ForbiddenAppException("This order does not belong to you.");

            order.Cancel();
            await repository.SaveChangesAsync(cancellationToken);

            // Invalidate rather than update in place — GetOrderById's read-through re-populates the cache on next read.
            await cache.RemoveAsync(OrderCacheKeys.Id(order.Id), cancellationToken);

            await eventPublisher.PublishAsync(
                new OrderCancelledEvent(order.Id, order.CustomerId, order.OrderNumber, "Cancelled by customer", DateTimeOffset.UtcNow),
                cancellationToken);

            return OrderDto.FromDomain(order);
        }
    }
}
