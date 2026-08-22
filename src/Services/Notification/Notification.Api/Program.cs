using ECommerce.BuildingBlocks.Auth;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Notification.Api.Consumers;
using Notification.Api.Hubs;
using Notification.Api.Middleware;
using Notification.Api.Services;
using RabbitMQ.Client;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.Services.AddJwtAuthentication(builder.Configuration);

// Browsers can't set an Authorization header on the WebSocket upgrade request SignalR uses, so
// the client instead sends the access token as a query string parameter (see NotificationsContext.jsx's
// accessTokenFactory) — this reads it back into the same bearer pipeline everything else uses.
builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                context.Token = accessToken;
            return Task.CompletedTask;
        },
    };
});

var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");

builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddScoped<INotificationStore, RedisNotificationStore>();
builder.Services.AddScoped<INotificationPusher, SignalRNotificationPusher>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddMassTransit(bus =>
{
    bus.AddConsumer<OrderConfirmedConsumer>();
    bus.AddConsumer<OrderCancelledConsumer>();
    bus.AddConsumer<PaymentSucceededConsumer>();
    bus.AddConsumer<PaymentFailedConsumer>();

    bus.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });

        cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddHealthChecks()
    .AddRedis(redisConnectionString, name: "redis")
    .AddRabbitMQ(sp =>
    {
        var factory = new ConnectionFactory
        {
            HostName = builder.Configuration["RabbitMq:Host"] ?? "localhost",
            UserName = builder.Configuration["RabbitMq:Username"] ?? "guest",
            Password = builder.Configuration["RabbitMq:Password"] ?? "guest",
        };
        return factory.CreateConnectionAsync(CancellationToken.None);
    }, name: "rabbitmq");

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
    app.MapScalarApiReference(); // http://localhost:5006/scalar
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
