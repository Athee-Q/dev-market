namespace Notification.Api.Services;

public interface INotificationService
{
    Task NotifyAsync(Guid customerId, string message, string type, CancellationToken ct);
}

/// <summary>Ties together Redis persistence and the live SignalR push (§12 flow: consumer -> Redis -> SignalR -> React).</summary>
public class NotificationService(INotificationStore store, INotificationPusher pusher) : INotificationService
{
    public async Task NotifyAsync(Guid customerId, string message, string type, CancellationToken ct)
    {
        var notification = await store.AddAsync(customerId, message, type, ct);
        await pusher.PushAsync(notification, ct);
    }
}
