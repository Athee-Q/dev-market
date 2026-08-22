using Customer.Application.Abstractions;
using ECommerce.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Customer.Infrastructure.Messaging.Consumers;

/// <summary>
/// Customer Service's first (and only) MassTransit consumer — creates the business-profile
/// Customer row for a freshly registered account, with Id == UserId so every other service
/// (Order, Cart, Payment) can keep treating "authenticated user" and "customer" as the same GUID.
/// Idempotent: a redelivered event just finds the row already there and does nothing.
/// </summary>
public class UserRegisteredConsumer(ICustomerRepository repository, ILogger<UserRegisteredConsumer> logger)
    : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;

        if (await repository.GetByIdAsync(message.UserId, context.CancellationToken) is not null)
        {
            logger.LogInformation("Customer {UserId} already exists — skipping (redelivered UserRegisteredEvent)", message.UserId);
            return;
        }

        var customer = new Domain.Customer(message.UserId, message.FullName, message.Email, phone: string.Empty);
        await repository.AddAsync(customer, context.CancellationToken);
        await repository.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Created Customer {UserId} from UserRegisteredEvent", message.UserId);
    }
}
