using Customer.Application.Abstractions;
using Customer.Infrastructure.Messaging.Consumers;
using Customer.Infrastructure.Persistence;
using Customer.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Customer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomerInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CustomerDb")
            ?? throw new InvalidOperationException("Connection string 'CustomerDb' is not configured.");

        services.AddDbContext<CustomerDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<ICustomerRepository, CustomerRepository>();

        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");

        // Backs Application's CachingBehaviour (see AddCustomerApplication) — GetCustomerById opts in
        // via ICacheableQuery; everything else passes through untouched.
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "customer:";
        });

        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<UserRegisteredConsumer>();

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
