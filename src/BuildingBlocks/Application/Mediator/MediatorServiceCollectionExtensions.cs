using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.BuildingBlocks.Application.Mediator;

public static class MediatorServiceCollectionExtensions
{
    /// <summary>Registers IMediator plus every IRequestHandler&lt;,&gt; implementation found in the given assemblies.</summary>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<IMediator, Mediator>();

        var handlerRegistrations =
            from assembly in assemblies
            from type in assembly.GetTypes()
            where !type.IsAbstract && !type.IsInterface
            from iface in type.GetInterfaces()
            where iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
            select (Service: iface, Implementation: type);

        foreach (var (service, implementation) in handlerRegistrations)
            services.AddTransient(service, implementation);

        return services;
    }

    /// <summary>Registers an open-generic pipeline behavior (e.g. ValidationBehaviour&lt;,&gt;) for every request/response pair.</summary>
    public static IServiceCollection AddPipelineBehavior(this IServiceCollection services, Type openBehaviorType)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), openBehaviorType);
        return services;
    }
}
