using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ECommerce.BuildingBlocks.WebApi;

public static class CorrelationIdMiddlewareExtensions
{
    /// <summary>
    /// Echoes/generates X-Correlation-Id and puts it in the logger scope for the rest of the
    /// request — identical inline middleware every service's Program.cs used to repeat.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
            context.Response.Headers["X-Correlation-Id"] = correlationId;
            using (app.Logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
            {
                await next();
            }
        });

        return app;
    }
}
