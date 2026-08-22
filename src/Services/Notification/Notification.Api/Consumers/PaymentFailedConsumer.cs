using ECommerce.Contracts.Events;
using MassTransit;
using Notification.Api.Services;

namespace Notification.Api.Consumers;

public class PaymentFailedConsumer(INotificationService notificationService) : IConsumer<PaymentFailedEvent>
{
    public Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var e = context.Message;
        return notificationService.NotifyAsync(
            e.CustomerId, $"Payment failed for order {e.OrderNumber}: {e.Reason}", "PaymentFailed", context.CancellationToken);
    }
}
