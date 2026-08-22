namespace ECommerce.Contracts.Events;

/// <summary>
/// Published by Identity Service right after a new user account is created. Customer Service
/// consumes this to create the matching business-profile row — Id = UserId, so "customer id" and
/// "authenticated user id" are the same GUID everywhere else in the system (Order, Cart, Payment).
/// </summary>
public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string FullName,
    DateTimeOffset RegisteredAt);
