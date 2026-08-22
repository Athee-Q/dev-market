using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ECommerce.BuildingBlocks.WebApi;

public static class EndpointExtensions
{
    /// <summary>Registers every IEndpoint implementation found in the given assembly.</summary>
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        var descriptors = assembly.GetTypes()
            .Where(type => !type.IsAbstract && !type.IsInterface && typeof(IEndpoint).IsAssignableFrom(type))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type));

        services.TryAddEnumerable(descriptors);
        return services;
    }

    /// <summary>Maps every registered IEndpoint — call once from Program.cs instead of MapControllers().</summary>
    public static IEndpointRouteBuilder MapEndpoints(this WebApplication app)
    {
        foreach (var endpoint in app.Services.GetRequiredService<IEnumerable<IEndpoint>>())
            endpoint.MapEndpoint(app);

        return app;
    }
}
