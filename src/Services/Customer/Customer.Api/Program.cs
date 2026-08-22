using Customer.Application;
using Customer.Infrastructure;
using Customer.Infrastructure.Persistence;
using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDefaultJsonOptions();

builder.Services.AddCustomerApplication();
builder.Services.AddCustomerInfrastructure(builder.Configuration);
builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("CustomerDb")!, name: "sqlserver")
    .AddRabbitMQ(sp =>
    {
        var factory = new ConnectionFactory
        {
            HostName = builder.Configuration["RabbitMq:Host"] ?? "localhost",
            UserName = builder.Configuration["RabbitMq:Username"] ?? "guest",
            Password = builder.Configuration["RabbitMq:Password"] ?? "guest",
        };
        return factory.CreateConnectionAsync(CancellationToken.None);
    }, name: "rabbitmq")
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!, name: "redis");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<CustomerDbContext>().Database.MigrateAsync();
}

app.UseCorrelationId();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // http://localhost:5002/scalar
}

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
