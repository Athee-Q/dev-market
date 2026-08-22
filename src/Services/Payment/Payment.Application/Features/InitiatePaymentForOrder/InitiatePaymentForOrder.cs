using ECommerce.BuildingBlocks.Application.Mediator;
using Payment.Application.Abstractions;

namespace Payment.Application.Features.InitiatePaymentForOrder;

/// <summary>Reacts to Order Service confirming an order: creates the Razorpay order the frontend will pay against.</summary>
public static class InitiatePaymentForOrder
{
    private const string Currency = "INR";

    public record Command(Guid OrderId, Guid CustomerId, string OrderNumber, decimal Amount) : IRequest<Unit>;

    public class Handler(IPaymentRepository repository, IPaymentGateway gateway) : IRequestHandler<Command, Unit>
    {
        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            if (await repository.GetByOrderIdAsync(request.OrderId, cancellationToken) is not null)
                return Unit.Value; // already processed this order — idempotent consumer, redelivery-safe

            var razorpayOrderId = await gateway.CreateOrderAsync(request.OrderId, request.Amount, Currency, cancellationToken);
            var payment = new Domain.Payment(request.OrderId, request.CustomerId, request.OrderNumber, request.Amount, Currency, razorpayOrderId);

            await repository.AddAsync(payment, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
