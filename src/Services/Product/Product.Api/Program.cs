using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Microsoft.EntityFrameworkCore;
using Product.Api.Services;
using Product.Application;
using Product.Infrastructure;
using Product.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// gRPC gets its own dedicated Kestrel endpoint (Kestrel:Endpoints:Grpc in appsettings, HTTP/2 only)
// rather than sharing the REST port with HttpProtocols.Http1AndHttp2 — Kestrel can't reliably
// negotiate HTTP/1.1 vs HTTP/2 on one plain-HTTP (non-TLS) port: without ALPN there's nothing to
// negotiate on, and a shared port intermittently answers gRPC calls with an "HTTP_1_1_REQUIRED"
// GOAWAY, which is the officially documented reason to split ports for non-TLS gRPC+REST hosting.
// NOTE: once appsettings defines ANY Kestrel:Endpoints entry, Kestrel stops honoring
// ASPNETCORE_URLS/launchSettings for the default endpoint entirely — so the REST endpoint (Http)
// must be declared explicitly there too, alongside Grpc. See appsettings.json/appsettings.Docker.json.

builder.Services.AddOpenApi();
builder.Services.AddDefaultJsonOptions();

builder.Services.AddProductApplication();
builder.Services.AddProductInfrastructure(builder.Configuration);
builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddGrpc();

builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("ProductDb")!, name: "sqlserver")
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis");

var app = builder.Build();

// Applies pending EF Core migrations on startup — convenient for a learning/demo Compose stack.
// A production deployment would run migrations as a separate release step instead.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<ProductDbContext>().Database.MigrateAsync();
}

app.UseCorrelationId();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // http://localhost:5001/scalar
}

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();
app.MapGrpcService<ProductCatalogGrpcService>();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
