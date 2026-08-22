using MassTransit;
using Payment.Application.Abstractions;

namespace Payment.Infrastructure.Messaging;

public class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken ct) where T : class =>
        publishEndpoint.Publish(integrationEvent, ct);
}
