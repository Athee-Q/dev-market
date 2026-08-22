using Notification.Api.Models;

namespace Notification.Api.Services;

public interface INotificationStore
{
    Task<NotificationDto> AddAsync(Guid customerId, string message, string type, CancellationToken ct);
    Task<IReadOnlyCollection<NotificationDto>> ListAsync(Guid customerId, bool onlyUnread, CancellationToken ct);
    Task<long> UnreadCountAsync(Guid customerId, CancellationToken ct);
    Task<bool> MarkReadAsync(Guid customerId, Guid notificationId, CancellationToken ct);
}
