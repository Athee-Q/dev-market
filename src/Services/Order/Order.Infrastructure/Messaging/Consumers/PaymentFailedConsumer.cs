using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Order.Application.Features.MarkOrderPaymentFailed;

namespace Order.Infrastructure.Messaging.Consumers;

/// <summary>
/// Reacts to Payment Service failing to verify a payment: moves the order to PaymentFailed
/// (retryable, not cancelled). Named distinctly from Notification's own PaymentFailedEvent
/// consumer — MassTransit's default endpoint-name formatter derives the RabbitMQ queue name from
/// the bare consumer class name (not the namespace/assembly), so two same-named consumers in
/// different services would silently compete on one shared queue instead of each getting their own.
/// </summary>
public class OrderPaymentFailedConsumer(IMediator mediator, ILogger<OrderPaymentFailedConsumer> logger)
    : IConsumer<PaymentFailedEvent>
{
    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var message = context.Message;
        logger.LogWarning("Payment failed for order {OrderId}: {Reason}", message.OrderId, message.Reason);

        await mediator.Send(new MarkOrderPaymentFailed.Command(message.OrderId, message.Reason), context.CancellationToken);
    }
}
