using ECommerce.Contracts.Events;
using MassTransit;
using Notification.Api.Services;

namespace Notification.Api.Consumers;

public class PaymentSucceededConsumer(INotificationService notificationService) : IConsumer<PaymentSucceededEvent>
{
    public Task Consume(ConsumeContext<PaymentSucceededEvent> context)
    {
        var e = context.Message;
        return notificationService.NotifyAsync(
            e.CustomerId, $"Payment received for order {e.OrderNumber}.", "PaymentSucceeded", context.CancellationToken);
    }
}
