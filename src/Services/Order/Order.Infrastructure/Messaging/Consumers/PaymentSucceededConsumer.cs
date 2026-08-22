using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Order.Application.Features.CompleteOrderAfterPayment;

namespace Order.Infrastructure.Messaging.Consumers;

/// <summary>
/// Reacts to Payment Service verifying a captured payment: moves the order to Completed. Named
/// distinctly from Notification's own PaymentSucceededEvent consumer — MassTransit's default
/// endpoint-name formatter derives the RabbitMQ queue name from the bare consumer class name
/// (not the namespace/assembly), so two same-named consumers in different services would
/// silently compete on one shared queue instead of each getting their own.
/// </summary>
public class OrderPaymentSucceededConsumer(IMediator mediator, ILogger<OrderPaymentSucceededConsumer> logger)
    : IConsumer<PaymentSucceededEvent>
{
    public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
    {
        var message = context.Message;
        logger.LogInformation("Payment succeeded for order {OrderId} (Razorpay payment {RazorpayPaymentId})", message.OrderId, message.RazorpayPaymentId);

        await mediator.Send(new CompleteOrderAfterPayment.Command(message.OrderId), context.CancellationToken);
    }
}
