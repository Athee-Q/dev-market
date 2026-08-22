using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Identity.Application;
using Identity.Application.Abstractions;
using Identity.Infrastructure;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDefaultJsonOptions();

builder.Services.AddIdentityApplication();
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddEndpoints(typeof(Program).Assembly);

// Identity Service both issues tokens (JwtTokenService, Infrastructure) and validates them for
// its own /me endpoint — every other service only ever does the latter.
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("IdentityDb")!, name: "sqlserver")
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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await db.Database.MigrateAsync();

    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await IdentitySeeder.SeedAsync(db, app.Configuration, passwordHasher, CancellationToken.None);
}

app.UseCorrelationId();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // http://localhost:5008/scalar
}

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
