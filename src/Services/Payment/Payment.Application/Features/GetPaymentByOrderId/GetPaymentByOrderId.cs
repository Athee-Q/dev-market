using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using Payment.Application.Abstractions;
using Payment.Application.Dto;

namespace Payment.Application.Features.GetPaymentByOrderId;

/// <summary>
/// The payment record for a confirmed order, including the Razorpay key id and order id the
/// frontend needs to open Checkout. Null until Payment Service has processed that order's
/// OrderConfirmedEvent — the frontend should retry briefly rather than treat this as fatal.
/// RequestingCustomerId/IsAdmin come from the caller's JWT — a customer can only fetch their own payment.
/// </summary>
public static class GetPaymentByOrderId
{
    public record Query(Guid OrderId, Guid RequestingCustomerId, bool IsAdmin) : IRequest<PaymentDto?>;

    public class Handler(IPaymentRepository repository, IPaymentGateway gateway) : IRequestHandler<Query, PaymentDto?>
    {
        public async Task<PaymentDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var payment = await repository.GetByOrderIdAsync(request.OrderId, cancellationToken);
            if (payment is null) return null;

            if (!request.IsAdmin && payment.CustomerId != request.RequestingCustomerId)
                throw new ForbiddenAppException("This payment does not belong to you.");

            return PaymentDto.FromDomain(payment, gateway.KeyId);
        }
    }
}
