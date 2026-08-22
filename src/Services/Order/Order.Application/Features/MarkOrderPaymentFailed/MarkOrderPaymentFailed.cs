using ECommerce.BuildingBlocks.Application.Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Order.Application.Abstractions;
using Order.Domain;

namespace Order.Application.Features.MarkOrderPaymentFailed;

/// <summary>Reacts to PaymentFailedEvent (via its consumer): moves the order to PaymentFailed (retryable, not cancelled).</summary>
public static class MarkOrderPaymentFailed
{
    public record Command(Guid OrderId, string Reason) : IRequest<Unit>;

    public class Handler(IOrderRepository repository, IDistributedCache cache) : IRequestHandler<Command, Unit>
    {
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var order = await repository.GetByIdAsync(request.OrderId, cancellationToken);
            // Completed already won a race with a later failure notification, or this is a redelivery — either way, no-op.
            if (order is null || order.Status is OrderStatus.Completed or OrderStatus.PaymentFailed) return Unit.Value;

            order.MarkPaymentFailed();
            await repository.SaveChangesAsync(cancellationToken);

            // Invalidate rather than update in place — GetOrderById's read-through re-populates the cache on next read.
            // Only reached on the actual mutating path, never on the idempotent no-op above.
            await cache.RemoveAsync(OrderCacheKeys.Id(request.OrderId), cancellationToken);

            return Unit.Value;
        }
    }
}
