using System.Text.Json;
using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using ECommerce.Contracts.Events;
using Payment.Application.Abstractions;
using Payment.Domain;

namespace Payment.Application.Features.HandleWebhook;

/// <summary>
/// Razorpay's server-to-server webhook (payment.captured / payment.failed) — a fallback for when
/// the browser never posts back to Verify (closed tab, network drop, ...). Needs a publicly
/// reachable URL to actually receive calls; see README for local-dev options.
/// </summary>
public static class HandleWebhook
{
    public record Command(string RawBody, string SignatureHeader) : IRequest<Unit>;

    public class Handler(IPaymentRepository repository, IPaymentGateway gateway, IEventPublisher eventPublisher) : IRequestHandler<Command, Unit>
    {
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            if (!gateway.VerifyWebhookSignature(request.RawBody, request.SignatureHeader))
                throw new ValidationAppException("Invalid webhook signature.");

            using var doc = JsonDocument.Parse(request.RawBody);
            var root = doc.RootElement;

            var eventName = root.TryGetProperty("event", out var eventProp) ? eventProp.GetString() : null;
            if (eventName is not ("payment.captured" or "payment.failed"))
                return Unit.Value; // other event types (order.paid, refund.*, ...) are out of scope for this demo

            var entity = root.GetProperty("payload").GetProperty("payment").GetProperty("entity");
            var razorpayOrderId = entity.GetProperty("order_id").GetString()!;
            var razorpayPaymentId = entity.GetProperty("id").GetString()!;

            var payment = await repository.GetByRazorpayOrderIdAsync(razorpayOrderId, cancellationToken);
            // Unknown order or already-settled payment — Razorpay retries webhook deliveries, so this
            // is a normal, non-error no-op rather than something to surface as a failure.
            if (payment is null || payment.Status == PaymentStatus.Succeeded)
                return Unit.Value;

            if (eventName == "payment.captured")
            {
                payment.MarkSucceeded(razorpayPaymentId);
                await repository.SaveChangesAsync(cancellationToken);

                await eventPublisher.PublishAsync(
                    new PaymentSucceededEvent(
                        payment.Id, payment.OrderId, payment.CustomerId, payment.OrderNumber,
                        payment.RazorpayOrderId, razorpayPaymentId, payment.Amount, DateTimeOffset.UtcNow),
                    cancellationToken);
            }
            else
            {
                var reason = entity.TryGetProperty("error_description", out var err) ? err.GetString() ?? "Payment failed" : "Payment failed";
                payment.MarkFailed(reason);
                await repository.SaveChangesAsync(cancellationToken);

                await eventPublisher.PublishAsync(
                    new PaymentFailedEvent(payment.Id, payment.OrderId, payment.CustomerId, payment.OrderNumber, reason, DateTimeOffset.UtcNow), cancellationToken);
            }

            return Unit.Value;
        }
    }
}
