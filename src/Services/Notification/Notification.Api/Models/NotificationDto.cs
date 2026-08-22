namespace Notification.Api.Models;

public record NotificationDto(
    Guid Id,
    Guid CustomerId,
    string Message,
    string Type,
    bool IsRead,
    DateTimeOffset CreatedAt);
