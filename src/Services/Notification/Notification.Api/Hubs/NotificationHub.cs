using ECommerce.BuildingBlocks.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Notification.Api.Hubs;

/// <summary>
/// Every connection auto-joins its own per-customer group, derived from the JWT (see Program.cs's
/// OnMessageReceived — the token travels as a query string param since WebSocket upgrades can't
/// carry an Authorization header) rather than a client-supplied id.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(Context.User!.GetUserId().ToString()));
        await base.OnConnectedAsync();
    }

    public static string GroupName(string customerId) => $"customer:{customerId}";
}
