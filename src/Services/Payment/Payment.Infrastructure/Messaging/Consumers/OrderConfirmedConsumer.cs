using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Payment.Application.Features.InitiatePaymentForOrder;

namespace Payment.Infrastructure.Messaging.Consumers;

/// <summary>
/// Reacts to Order Service confirming an order: creates the Razorpay order the frontend will pay
/// against. Named distinctly from Notification's own OrderConfirmedEvent consumer — MassTransit's
/// default endpoint-name formatter derives the RabbitMQ queue name from the bare consumer class
/// name (not the namespace/assembly), so two same-named consumers in different services would
/// silently compete on one shared queue instead of each getting their own.
/// </summary>
public class PaymentOrderConfirmedConsumer(IMediator mediator, ILogger<PaymentOrderConfirmedConsumer> logger)
    : IConsumer<OrderConfirmedEvent>
{
    public async Task Consume(ConsumeContext<OrderConfirmedEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Initiating payment for order {OrderId} ({OrderNumber}), amount {Amount}", message.OrderId, message.OrderNumber, message.TotalAmount);

        await mediator.Send(
            new InitiatePaymentForOrder.Command(message.OrderId, message.CustomerId, message.OrderNumber, message.TotalAmount),
            context.CancellationToken);
    }
}
