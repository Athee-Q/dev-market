using ECommerce.Contracts.Events;
using MassTransit;
using Notification.Api.Services;

namespace Notification.Api.Consumers;

public class OrderCancelledConsumer(INotificationService notificationService) : IConsumer<OrderCancelledEvent>
{
    public Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        var e = context.Message;
        return notificationService.NotifyAsync(
            e.CustomerId, $"Order {e.OrderNumber} was cancelled: {e.Reason}", "OrderCancelled", context.CancellationToken);
    }
}
