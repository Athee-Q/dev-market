using ECommerce.BuildingBlocks.Application.Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Order.Application.Abstractions;
using Order.Domain;

namespace Order.Application.Features.CompleteOrderAfterPayment;

/// <summary>Reacts to PaymentSucceededEvent (via its consumer): moves the order to Completed.</summary>
public static class CompleteOrderAfterPayment
{
    public record Command(Guid OrderId) : IRequest<Unit>;

    public class Handler(IOrderRepository repository, IDistributedCache cache) : IRequestHandler<Command, Unit>
    {
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var order = await repository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null || order.Status == OrderStatus.Completed) return Unit.Value; // idempotent — event may be redelivered

            order.Complete();
            await repository.SaveChangesAsync(cancellationToken);

            // Invalidate rather than update in place — GetOrderById's read-through re-populates the cache on next read.
            // Only reached on the actual mutating path, never on the idempotent no-op above.
            await cache.RemoveAsync(OrderCacheKeys.Id(request.OrderId), cancellationToken);

            return Unit.Value;
        }
    }
}
