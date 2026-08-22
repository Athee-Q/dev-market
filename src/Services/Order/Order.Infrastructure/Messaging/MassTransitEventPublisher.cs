using MassTransit;
using Order.Application.Abstractions;

namespace Order.Infrastructure.Messaging;

public class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken ct) where T : class =>
        publishEndpoint.Publish(integrationEvent, ct);
}
