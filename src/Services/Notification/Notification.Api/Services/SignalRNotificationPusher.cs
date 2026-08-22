using Microsoft.AspNetCore.SignalR;
using Notification.Api.Hubs;
using Notification.Api.Models;

namespace Notification.Api.Services;

public class SignalRNotificationPusher(IHubContext<NotificationHub> hubContext) : INotificationPusher
{
    public Task PushAsync(NotificationDto notification, CancellationToken ct) =>
        hubContext.Clients
            .Group(NotificationHub.GroupName(notification.CustomerId.ToString()))
            .SendAsync("notificationReceived", notification, ct);
}
