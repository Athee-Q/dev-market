namespace ECommerce.Contracts.Events;

/// <summary>
/// Published by Notification Service after it stores a user-facing notification, in case other
/// consumers (e.g. an email/SMS worker added later) want to react to it. Not required for the
/// core SignalR push, which Notification Service delivers directly.
/// </summary>
public record NotificationCreatedEvent(
    Guid NotificationId,
    Guid CustomerId,
    string Message,
    string Type,
    DateTimeOffset CreatedAt);
