using Notification.Api.Models;

namespace Notification.Api.Services;

public interface INotificationPusher
{
    Task PushAsync(NotificationDto notification, CancellationToken ct);
}
