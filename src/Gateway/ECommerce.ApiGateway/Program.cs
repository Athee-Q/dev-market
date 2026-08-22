using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Single public entry point (§4): the browser only ever talks to the Gateway, so CORS and rate
// limiting are configured here once instead of on every downstream service.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var rateLimitOptions = builder.Configuration.GetSection("RateLimiting");
var globalPermitLimit = rateLimitOptions.GetValue("GlobalPermitLimit", 200);
var globalWindowSeconds = rateLimitOptions.GetValue("GlobalWindowSeconds", 60);
var writesPermitLimit = rateLimitOptions.GetValue("WritesPermitLimit", 20);
var writesWindowSeconds = rateLimitOptions.GetValue("WritesWindowSeconds", 60);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            status = StatusCodes.Status429TooManyRequests,
            title = "Too many requests",
            detail = "Rate limit exceeded. Please slow down and try again shortly.",
        }, ct);
    };

    // Default policy for every request through the Gateway, partitioned per client so one
    // caller can't starve everyone else. Generous — this is the safety net for browsing traffic.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ClientKey(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = globalPermitLimit,
                Window = TimeSpan.FromSeconds(globalWindowSeconds),
                QueueLimit = 0,
            }));

    // Tighter policy for state-changing routes (order/cart writes trigger the full saga and a
    // DB write) — opted into per-route via ReverseProxy:Routes:*:RateLimiterPolicy in appsettings.
    options.AddPolicy("writes", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ClientKey(httpContext),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = writesPermitLimit,
                Window = TimeSpan.FromSeconds(writesWindowSeconds),
                QueueLimit = 0,
            }));
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors("Frontend");
app.UseRateLimiter();

app.MapHealthChecks("/health");

// YARP forwards HTTP and WebSocket traffic (SignalR's /hubs/notifications route) alike.
app.MapReverseProxy();

app.Run();

// No auth yet (§17 is Phase 2), so the fairest per-client key available today is the caller's
// IP. Swap this for the authenticated user id once JWT auth lands.
static string ClientKey(HttpContext context) =>
    context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
