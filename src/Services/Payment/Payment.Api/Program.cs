using ECommerce.BuildingBlocks.Auth;
using ECommerce.BuildingBlocks.WebApi;
using Microsoft.EntityFrameworkCore;
using Payment.Application;
using Payment.Infrastructure;
using Payment.Infrastructure.Persistence;
using RabbitMQ.Client;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDefaultJsonOptions();

builder.Services.AddPaymentApplication();
builder.Services.AddPaymentInfrastructure(builder.Configuration);
builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("PaymentDb")!, name: "sqlserver")
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

// Applies pending EF Core migrations on startup — convenient for a learning/demo Compose stack.
// A production deployment would run migrations as a separate release step instead.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<PaymentDbContext>().Database.MigrateAsync();
}

app.UseCorrelationId();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // http://localhost:5007/scalar
}

app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
