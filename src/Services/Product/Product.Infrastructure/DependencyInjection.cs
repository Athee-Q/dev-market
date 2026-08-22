using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.Application.Abstractions;
using Product.Infrastructure.Caching;
using Product.Infrastructure.Persistence;
using Product.Infrastructure.Repositories;
using StackExchange.Redis;

namespace Product.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProductInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ProductDb")
            ?? throw new InvalidOperationException("Connection string 'ProductDb' is not configured.");

        services.AddDbContext<ProductDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IProductRepository, ProductRepository>();

        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");

        // Backs Application's CachingBehaviour/HybridCachingBehaviour (see AddProductApplication) —
        // GetProductById and SearchProducts opt in via ICacheableQuery/IHybridCacheableQuery;
        // everything else passes through untouched.
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "product:";
        });

        // Separate connection for Redis pub/sub (cross-instance L1 invalidation) — IDistributedCache
        // above doesn't expose ISubscriber. Same connection string, its own logical connection.
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddScoped<IProductCacheInvalidator, RedisProductCacheInvalidator>();
        services.AddHostedService<ProductCacheInvalidationSubscriber>();

        return services;
    }
}
