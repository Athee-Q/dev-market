using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using ECommerce.Contracts.Events;
using Payment.Application.Abstractions;
using Payment.Application.Dto;
using Payment.Domain;

namespace Payment.Application.Features.CheckUpiQrPayment;

/// <summary>
/// Polled by the frontend every couple of seconds while a UPI QR is on screen (see CreateUpiQr) —
/// there's no client-side handler callback for a QR scan the way Checkout provides one, so this
/// is how a QR payment gets noticed instead of only ever relying on the webhook.
/// </summary>
public static class CheckUpiQrPayment
{
    public record Query(Guid OrderId, Guid RequestingCustomerId, bool IsAdmin) : IRequest<PaymentDto>;

    public class Handler(IPaymentRepository repository, IPaymentGateway gateway, IEventPublisher eventPublisher)
        : IRequestHandler<Query, PaymentDto>
    {
        public async Task<PaymentDto> Handle(Query request, CancellationToken cancellationToken)
        {
            var payment = await repository.GetByOrderIdAsync(request.OrderId, cancellationToken)
                ?? throw new NotFoundException($"No payment found for order '{request.OrderId}'.");

            if (!request.IsAdmin && payment.CustomerId != request.RequestingCustomerId)
                throw new ForbiddenAppException("This payment does not belong to you.");

            if (payment.Status == PaymentStatus.Created && payment.RazorpayQrCodeId is { } qrCodeId)
            {
                var paidPaymentId = await gateway.GetQrCodePaymentIdAsync(qrCodeId, cancellationToken);
                if (paidPaymentId is not null)
                {
                    payment.MarkSucceeded(paidPaymentId);
                    await repository.SaveChangesAsync(cancellationToken);

                    await eventPublisher.PublishAsync(
                        new PaymentSucceededEvent(
                            payment.Id, payment.OrderId, payment.CustomerId, payment.OrderNumber,
                            payment.RazorpayOrderId, paidPaymentId, payment.Amount, DateTimeOffset.UtcNow),
                        cancellationToken);
                }
            }

            return PaymentDto.FromDomain(payment, gateway.KeyId);
        }
    }
}
