using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using ECommerce.Contracts.Events;
using FluentValidation;
using Order.Application.Abstractions;
using Order.Application.Dto;

namespace Order.Application.Features.CreateOrder;

public static class CreateOrder
{
    public record ItemInput(Guid ProductId, int Quantity);

    public record Command(Guid CustomerId, IReadOnlyCollection<ItemInput> Items) : IRequest<OrderDto>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.CustomerId).NotEmpty();
            RuleFor(x => x.Items).NotEmpty();
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).NotEmpty();
                item.RuleFor(i => i.Quantity).GreaterThan(0);
            });
        }
    }

    public class Handler(IOrderRepository repository, IProductCatalogClient productCatalogClient, IEventPublisher eventPublisher)
        : IRequestHandler<Command, OrderDto>
    {
        public async Task<OrderDto> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.Items.Count == 0)
                throw new ValidationAppException("An order must contain at least one item.");

            var lines = new List<(Guid ProductId, int Quantity, decimal UnitPrice, string ProductName, string ProductType, string? AssetUrl)>();
            foreach (var item in request.Items)
            {
                var product = await productCatalogClient.GetProductAsync(item.ProductId, cancellationToken)
                    ?? throw new ValidationAppException($"Product '{item.ProductId}' does not exist.");

                if (!product.IsActive)
                    throw new ValidationAppException($"Product '{product.Name}' is not currently available.");

                lines.Add((product.Id, item.Quantity, product.Price, product.Name, product.ProductType, product.AssetUrl));
            }

            // Digital goods have no stock to reserve, so the order confirms immediately —
            // there's no inventory-reservation saga step to wait on (see README "Vertical Slice
            // Architecture" for the physical-goods saga this replaced).
            var order = new Domain.Order(request.CustomerId, lines);
            order.Confirm();
            await repository.AddAsync(order, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            // NOTE: this is a simple publish, not a transactional outbox. If the process crashes
            // between SaveChangesAsync and PublishAsync the event is lost and the order stalls in
            // Confirmed with no payment ever initiated — see README "Known Gaps" for the outbox
            // pattern to close this in a later phase.
            await eventPublisher.PublishAsync(
                new OrderConfirmedEvent(order.Id, order.CustomerId, order.OrderNumber, order.TotalAmount, order.UpdatedAt),
                cancellationToken);

            return OrderDto.FromDomain(order);
        }
    }
}
