using ECommerce.Grpc.ProductCatalog;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Abstractions;
using Order.Infrastructure.ExternalServices;
using Order.Infrastructure.Messaging;
using Order.Infrastructure.Messaging.Consumers;
using Order.Infrastructure.Persistence;
using Order.Infrastructure.Repositories;

namespace Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Product.Api serves gRPC over plain HTTP/2 (h2c, no TLS between services in this Docker
        // network) — HttpClient/SocketsHttpHandler refuses unencrypted HTTP/2 by default, so this
        // opts in. Must be set before any gRPC channel is created.
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        var connectionString = configuration.GetConnectionString("OrderDb")
            ?? throw new InvalidOperationException("Connection string 'OrderDb' is not configured.");

        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");

        // Backs Application's CachingBehaviour (SearchOrders) and GetOrderById's manual read-through.
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "order:";
        });

        services.AddGrpcClient<ProductCatalog.ProductCatalogClient>(options =>
        {
            // Product.Api's dedicated gRPC (HTTP/2-only) Kestrel endpoint — see its Kestrel:Endpoints:Grpc
            // config and Program.cs comment for why this is a separate port from Services:ProductApi (REST).
            var baseUrl = configuration["Services:ProductGrpc"]
                ?? throw new InvalidOperationException("Configuration 'Services:ProductGrpc' is not set.");
            options.Address = new Uri(baseUrl);
        });
        services.AddScoped<IProductCatalogClient, GrpcProductCatalogClient>();

        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<OrderPaymentSucceededConsumer>();
            bus.AddConsumer<OrderPaymentFailedConsumer>();

            bus.UsingRabbitMq((context, cfg) =>
            {
                var rabbitHost = configuration["RabbitMq:Host"] ?? "localhost";
                cfg.Host(rabbitHost, "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? "guest");
                    h.Password(configuration["RabbitMq:Password"] ?? "guest");
                });

                cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
