using ECommerce.BuildingBlocks.Application.Behaviors;
using ECommerce.BuildingBlocks.Application.Mediator;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Product.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProductApplication(this IServiceCollection services)
    {
        services.AddMediator(typeof(DependencyInjection).Assembly);
        services.AddPipelineBehavior(typeof(ValidationBehaviour<,>));
        services.AddPipelineBehavior(typeof(CachingBehaviour<,>)); // SearchProducts opts in (L2 only), via ICacheableQuery
        services.AddPipelineBehavior(typeof(HybridCachingBehaviour<,>)); // GetProductById opts in (L1+L2), via IHybridCacheableQuery
        services.AddMemoryCache(); // backs HybridCachingBehaviour's L1 tier

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
