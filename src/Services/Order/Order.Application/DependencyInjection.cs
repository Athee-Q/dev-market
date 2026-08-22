using ECommerce.BuildingBlocks.Application.Behaviors;
using ECommerce.BuildingBlocks.Application.Mediator;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Order.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderApplication(this IServiceCollection services)
    {
        services.AddMediator(typeof(DependencyInjection).Assembly);
        services.AddPipelineBehavior(typeof(ValidationBehaviour<,>));
        services.AddPipelineBehavior(typeof(CachingBehaviour<,>)); // only SearchOrders opts in, via ICacheableQuery — GetOrderById does its own read-through (see GetOrderById.Handler)

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
