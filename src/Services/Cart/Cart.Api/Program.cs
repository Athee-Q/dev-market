using Cart.Api.ExternalServices;
using Cart.Api.Middleware;
using Cart.Api.Services;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.Grpc.ProductCatalog;
using Scalar.AspNetCore;
using StackExchange.Redis;

// Product.Api serves gRPC over plain HTTP/2 (h2c, no TLS between services in this Docker network)
// — HttpClient/SocketsHttpHandler refuses unencrypted HTTP/2 by default, so this opts in. Must be
// set before any gRPC channel is created.
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddJwtAuthentication(builder.Configuration);

var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");

builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddScoped<ICartService, RedisCartService>();

// Backs CachedProductCatalogClient's cache-aside wrapper around the outbound Product lookup below.
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "cart:";
});

builder.Services.AddGrpcClient<ProductCatalog.ProductCatalogClient>(options =>
{
    // Product.Api's dedicated gRPC (HTTP/2-only) Kestrel endpoint — see its Kestrel:Endpoints:Grpc
    // config and Program.cs comment for why this is a separate port from Services:ProductApi (REST).
    var baseUrl = builder.Configuration["Services:ProductGrpc"]
        ?? throw new InvalidOperationException("Configuration 'Services:ProductGrpc' is not set.");
    options.Address = new Uri(baseUrl);
});
builder.Services.AddScoped<GrpcProductCatalogClient>();
builder.Services.AddScoped<IProductCatalogClient, CachedProductCatalogClient>();

builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddRedis(redisConnectionString, name: "redis");

var app = builder.Build();

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    context.Response.Headers["X-Correlation-Id"] = correlationId;
    using (app.Logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
    {
        await next();
    }
});

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // http://localhost:5003/scalar
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
