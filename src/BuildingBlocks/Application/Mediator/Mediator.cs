using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.BuildingBlocks.Application.Mediator;

/// <summary>
/// Resolves the one IRequestHandler for the request's runtime type via DI, wraps it with every
/// registered IPipelineBehavior for that request/response pair, and invokes the chain. `dynamic`
/// is used at the two call sites where the request's exact compile-time type isn't known here
/// (only its base IRequest&lt;TResponse&gt;) — a standard, small technique for a hand-rolled
/// mediator, trading a little runtime dispatch cost for avoiding a wall of reflection.
/// </summary>
public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        dynamic handler = serviceProvider.GetRequiredService(handlerType);

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviors = serviceProvider.GetServices(behaviorType).Cast<dynamic>().Reverse();

        RequestHandlerDelegate<TResponse> pipeline = () => handler.Handle((dynamic)request, cancellationToken);

        foreach (var behavior in behaviors)
        {
            var next = pipeline;
            pipeline = () => behavior.Handle((dynamic)request, next, cancellationToken);
        }

        return pipeline();
    }
}
