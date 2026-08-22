using ECommerce.Contracts.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ECommerce.BuildingBlocks.WebApi;

/// <summary>
/// One exception handler shared by every service — the five per-service copies of this were
/// byte-for-byte identical except Inventory's extra ConcurrencyConflictException case, which now
/// lives in ECommerce.Contracts.Common so this single handler covers everyone.
/// </summary>
public class AppExceptionHandler(ILogger<AppExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            UnauthorizedAppException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ForbiddenAppException => (StatusCodes.Status403Forbidden, "Forbidden"),
            ConflictException or ConcurrencyConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            ValidationAppException => (StatusCodes.Status400BadRequest, "Validation failed"),
            FluentValidation.ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred"),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path,
        }, cancellationToken);

        return true;
    }
}
