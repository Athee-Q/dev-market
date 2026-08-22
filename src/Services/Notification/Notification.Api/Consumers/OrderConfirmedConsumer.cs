using ECommerce.Contracts.Events;
using MassTransit;
using Notification.Api.Services;

namespace Notification.Api.Consumers;

public class OrderConfirmedConsumer(INotificationService notificationService) : IConsumer<OrderConfirmedEvent>
{
    public Task Consume(ConsumeContext<OrderConfirmedEvent> context)
    {
        var e = context.Message;
        return notificationService.NotifyAsync(
            e.CustomerId, $"Order {e.OrderNumber} confirmed successfully.", "OrderConfirmed", context.CancellationToken);
    }
}
