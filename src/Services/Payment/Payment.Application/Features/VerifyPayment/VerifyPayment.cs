using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using ECommerce.Contracts.Events;
using FluentValidation;
using Payment.Application.Abstractions;
using Payment.Application.Dto;
using Payment.Domain;

namespace Payment.Application.Features.VerifyPayment;

/// <summary>Verifies the signature Razorpay Checkout returns to the browser on success and settles the payment.</summary>
public static class VerifyPayment
{
    public record Command(
        Guid OrderId, string RazorpayOrderId, string RazorpayPaymentId, string RazorpaySignature,
        Guid RequestingCustomerId, bool IsAdmin) : IRequest<PaymentDto>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId).NotEmpty();
            RuleFor(x => x.RazorpayOrderId).NotEmpty();
            RuleFor(x => x.RazorpayPaymentId).NotEmpty();
            RuleFor(x => x.RazorpaySignature).NotEmpty();
        }
    }

    public class Handler(IPaymentRepository repository, IPaymentGateway gateway, IEventPublisher eventPublisher)
        : IRequestHandler<Command, PaymentDto>
    {
        public async Task<PaymentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var payment = await repository.GetByOrderIdAsync(request.OrderId, cancellationToken)
                ?? throw new NotFoundException($"No payment found for order '{request.OrderId}'.");

            if (!request.IsAdmin && payment.CustomerId != request.RequestingCustomerId)
                throw new ForbiddenAppException("This payment does not belong to you.");

            if (payment.RazorpayOrderId != request.RazorpayOrderId)
                throw new ValidationAppException("Razorpay order id does not match this payment.");

            // Idempotent: the webhook may have already settled this payment before the browser's
            // callback made it back to us.
            if (payment.Status == PaymentStatus.Succeeded)
                return PaymentDto.FromDomain(payment, gateway.KeyId);

            if (!gateway.VerifyCheckoutSignature(request.RazorpayOrderId, request.RazorpayPaymentId, request.RazorpaySignature))
            {
                const string reason = "Payment signature verification failed.";
                payment.MarkFailed(reason);
                await repository.SaveChangesAsync(cancellationToken);

                await eventPublisher.PublishAsync(
                    new PaymentFailedEvent(payment.Id, payment.OrderId, payment.CustomerId, payment.OrderNumber, reason, DateTimeOffset.UtcNow), cancellationToken);

                throw new ValidationAppException(reason);
            }

            payment.MarkSucceeded(request.RazorpayPaymentId);
            await repository.SaveChangesAsync(cancellationToken);

            await eventPublisher.PublishAsync(
                new PaymentSucceededEvent(
                    payment.Id, payment.OrderId, payment.CustomerId, payment.OrderNumber,
                    payment.RazorpayOrderId, request.RazorpayPaymentId, payment.Amount, DateTimeOffset.UtcNow),
                cancellationToken);

            return PaymentDto.FromDomain(payment, gateway.KeyId);
        }
    }
}
