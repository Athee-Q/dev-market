using ECommerce.BuildingBlocks.Application.Behaviors;
using ECommerce.BuildingBlocks.Application.Mediator;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Customer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomerApplication(this IServiceCollection services)
    {
        services.AddMediator(typeof(DependencyInjection).Assembly);
        services.AddPipelineBehavior(typeof(ValidationBehaviour<,>));
        services.AddPipelineBehavior(typeof(CachingBehaviour<,>)); // only GetCustomerById opts in, via ICacheableQuery

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
