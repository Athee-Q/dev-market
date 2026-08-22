namespace ECommerce.Contracts.Common;

/// <summary>Thrown when a requested resource does not exist. Mapped to HTTP 404 ProblemDetails.</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>Thrown on a business-rule conflict (duplicate SKU, insufficient stock, ...). Mapped to HTTP 409.</summary>
public class ConflictException(string message) : Exception(message);

/// <summary>Thrown when a request fails validation outside FluentValidation's pipeline. Mapped to HTTP 400.</summary>
public class ValidationAppException(string message) : Exception(message);

/// <summary>Thrown when an optimistic-concurrency (rowversion) check fails on a write. Mapped to HTTP 409.</summary>
public class ConcurrencyConflictException(string message) : Exception(message);

/// <summary>Thrown for bad credentials or an invalid/expired refresh token. Mapped to HTTP 401.</summary>
public class UnauthorizedAppException(string message) : Exception(message);

/// <summary>Thrown when an authenticated caller tries to act on a resource they don't own and lack the admin permission for. Mapped to HTTP 403.</summary>
public class ForbiddenAppException(string message) : Exception(message);
