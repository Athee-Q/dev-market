using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.BuildingBlocks.WebApi;

public static class JsonOptionsExtensions
{
    /// <summary>
    /// Serializes enums (OrderStatus, PaymentStatus, ...) as their string name instead of the
    /// numeric default. Minimal APIs read Microsoft.AspNetCore.Http.Json.JsonOptions — a
    /// different options object from the Microsoft.AspNetCore.Mvc.JsonOptions Controllers used
    /// to read — so this has to be configured explicitly now; the frontend depends on it
    /// (`order.status.toLowerCase()` and friends).
    /// </summary>
    public static IServiceCollection AddDefaultJsonOptions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}
