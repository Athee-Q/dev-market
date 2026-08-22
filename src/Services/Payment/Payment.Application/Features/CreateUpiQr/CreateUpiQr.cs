using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using Payment.Application.Abstractions;
using Payment.Application.Dto;
using Payment.Domain;

namespace Payment.Application.Features.CreateUpiQr;

/// <summary>
/// Headless alternative to Razorpay Checkout (see OrderDetailsPage's "Pay Now" for the Checkout
/// path): mints a single-use, fixed-amount UPI QR code the frontend renders directly, with no
/// popup. RequestingCustomerId/IsAdmin come from the caller's JWT.
/// </summary>
public static class CreateUpiQr
{
    public record Command(Guid OrderId, Guid RequestingCustomerId, bool IsAdmin) : IRequest<UpiQrDto>;

    public class Handler(IPaymentRepository repository, IPaymentGateway gateway) : IRequestHandler<Command, UpiQrDto>
    {
        public async Task<UpiQrDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var payment = await repository.GetByOrderIdAsync(request.OrderId, cancellationToken)
                ?? throw new NotFoundException($"No payment found for order '{request.OrderId}'.");

            if (!request.IsAdmin && payment.CustomerId != request.RequestingCustomerId)
                throw new ForbiddenAppException("This payment does not belong to you.");

            if (payment.Status == PaymentStatus.Succeeded)
                throw new ConflictException("This order has already been paid.");

            // Reuse an existing, still-valid QR instead of minting a new one on every click/poll.
            if (payment.RazorpayQrCodeId is null || payment.UpiQrExpiresAt is not { } expiresAt || expiresAt <= DateTimeOffset.UtcNow)
            {
                var (qrCodeId, imageUrl, newExpiresAt) = await gateway.CreateUpiQrCodeAsync(payment.OrderId, payment.Amount, cancellationToken);
                payment.AttachUpiQr(qrCodeId, imageUrl, newExpiresAt);
                await repository.SaveChangesAsync(cancellationToken);
            }

            return new UpiQrDto(payment.RazorpayQrCodeId!, payment.UpiQrImageUrl!, payment.UpiQrExpiresAt!.Value);
        }
    }
}
